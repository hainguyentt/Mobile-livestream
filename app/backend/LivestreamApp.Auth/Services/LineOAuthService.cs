using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Auth.Options;
using LivestreamApp.Auth.Repositories;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LivestreamApp.Auth.Services;

public class LineOAuthService : ILineOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly LineOptions _options;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LineOAuthService(
        HttpClient httpClient,
        IOptions<LineOptions> options,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public string GetAuthorizationUrl(string state)
    {
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        query["response_type"] = "code";
        query["client_id"] = _options.ClientId;
        query["redirect_uri"] = _options.RedirectUri;
        query["state"] = state;
        query["scope"] = "profile openid email";
        return $"https://access.line.me/oauth2/v2.1/authorize?{query}";
    }

    public async Task<LineTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret
        };

        var response = await _httpClient.PostAsync(
            "https://api.line.me/oauth2/v2.1/token",
            new FormUrlEncodedContent(form), ct);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LineTokenApiResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Failed to parse LINE token response.");

        return new LineTokenResponse(result.AccessToken, result.IdToken ?? string.Empty, result.RefreshToken ?? string.Empty);
    }

    public async Task<LineUserProfile> GetUserProfileAsync(string accessToken, CancellationToken ct = default)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var result = await _httpClient.GetFromJsonAsync<LineProfileApiResponse>(
            "https://api.line.me/v2/profile", ct)
            ?? throw new InvalidOperationException("Failed to parse LINE profile response.");

        return new LineUserProfile(result.UserId, result.DisplayName, result.Email, result.PictureUrl);
    }

    public async Task<User> LinkOrMergeAccountAsync(string lineUserId, string? lineEmail, string displayName, CancellationToken ct = default)
    {
        // Check if LINE account already linked
        var existingByLine = await _userRepository.GetByExternalLoginAsync("LINE", lineUserId, ct);
        if (existingByLine is not null)
            return existingByLine;

        // Check if email account exists — merge
        if (!string.IsNullOrEmpty(lineEmail))
        {
            var existingByEmail = await _userRepository.GetByEmailAsync(lineEmail, ct);
            if (existingByEmail is not null)
            {
                var login = ExternalLogin.Create(existingByEmail.Id, "LINE", lineUserId, lineEmail);
                await _userRepository.AddExternalLoginAsync(login, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                return existingByEmail;
            }
        }

        // Create new account
        var email = lineEmail ?? $"line_{lineUserId}@line.placeholder";
        var newUser = User.CreateFromExternalLogin(email);
        var externalLogin = ExternalLogin.Create(newUser.Id, "LINE", lineUserId, lineEmail);

        await _userRepository.AddAsync(newUser, ct);
        await _userRepository.AddExternalLoginAsync(externalLogin, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return newUser;
    }

    // Internal API response models
    private record LineTokenApiResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("id_token")] string? IdToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken);

    private record LineProfileApiResponse(
        [property: JsonPropertyName("userId")] string UserId,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("pictureUrl")] string? PictureUrl);
}
