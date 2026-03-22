using LivestreamApp.API.DTOs.Auth;
using LivestreamApp.Auth.Services;
using LivestreamApp.Shared.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestreamApp.API.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Registers a new user with email and password. Sends OTP verification email.</summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var user = await _authService.RegisterWithEmailAsync(request.Email, request.Password, ct);
        return Ok(new { message = "Registration successful. Please verify your email.", userId = user.Id });
    }

    /// <summary>Authenticates with email/password. Sets httpOnly access_token cookie.</summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var ip = GetClientIp();
        var tokens = await _authService.LoginWithEmailAsync(request.Email, request.Password, ip, ct);
        SetTokenCookies(tokens.AccessToken, tokens.RefreshToken);
        return Ok(new { message = "Login successful." });
    }

    /// <summary>Initiates LINE OAuth login. Returns redirect URL.</summary>
    [HttpPost("login/line")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LoginWithLine([FromBody] LineLoginRequest request, CancellationToken ct)
    {
        var ip = GetClientIp();
        var tokens = await _authService.LoginWithLineAsync(request.Code, ip, ct);
        SetTokenCookies(tokens.AccessToken, tokens.RefreshToken);
        return Ok(new { message = "LINE login successful." });
    }

    /// <summary>Rotates refresh token. Reads from httpOnly cookie.</summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { error = "unauthorized", message = "No refresh token provided." });

        var ip = GetClientIp();
        var tokens = await _authService.RefreshAccessTokenAsync(refreshToken, ip, ct);
        SetTokenCookies(tokens.AccessToken, tokens.RefreshToken);
        return Ok(new { message = "Token refreshed." });
    }

    /// <summary>Revokes refresh token and clears cookies.</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.RevokeRefreshTokenAsync(refreshToken, ct);

        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>Sends email OTP for verification or password reset.</summary>
    [HttpPost("otp/email/send")]
    public async Task<IActionResult> SendEmailOtp([FromBody] SendOtpRequest request, CancellationToken ct)
    {
        var purpose = Enum.Parse<OtpPurpose>(request.Purpose);
        await _authService.SendEmailOtpAsync(request.Target, purpose, ct);
        return Ok(new { message = "OTP sent to email." });
    }

    /// <summary>Verifies email OTP code.</summary>
    [HttpPost("otp/email/verify")]
    public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        var purpose = Enum.Parse<OtpPurpose>(request.Purpose);
        var result = await _authService.VerifyEmailOtpAsync(request.Target, request.Code, purpose, ct);
        if (!result)
            return BadRequest(new { error = "invalid_otp", message = "Invalid or expired OTP code." });

        return Ok(new { message = "Email verified successfully." });
    }

    /// <summary>Sends phone OTP for verification.</summary>
    [HttpPost("otp/phone/send")]
    [Authorize]
    public async Task<IActionResult> SendPhoneOtp([FromBody] SendOtpRequest request, CancellationToken ct)
    {
        await _authService.SendPhoneOtpAsync(request.Target, ct);
        return Ok(new { message = "OTP sent to phone." });
    }

    /// <summary>Verifies phone OTP code.</summary>
    [HttpPost("otp/phone/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyPhoneOtp([FromBody] VerifyOtpRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var result = await _authService.VerifyPhoneOtpAsync(userId, request.Target, request.Code, ct);
        if (!result)
            return BadRequest(new { error = "invalid_otp", message = "Invalid or expired OTP code." });

        return Ok(new { message = "Phone verified successfully." });
    }

    /// <summary>Resets password using OTP code.</summary>
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
    {
        await _authService.ResetPasswordAsync(request.Email, request.NewPassword, request.OtpCode, ct);
        return Ok(new { message = "Password reset successfully." });
    }

    // --- Private helpers ---

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        // Secure=false in non-HTTPS environments (test/dev without HTTPS)
        var isSecure = Request.IsHttps;

        var baseOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax
        };

        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            MaxAge = TimeSpan.FromMinutes(15)
        });
        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = baseOptions.HttpOnly,
            Secure = baseOptions.Secure,
            SameSite = baseOptions.SameSite,
            MaxAge = TimeSpan.FromDays(30)
        });
    }

    private string GetClientIp() =>
        Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').First().Trim()
        ?? HttpContext.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}

// Inline DTO for LINE login (only used here)
public record LineLoginRequest(string Code);
