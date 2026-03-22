using FluentAssertions;
using LivestreamApp.API.Controllers.V1;
using LivestreamApp.API.DTOs.Auth;
using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Auth.Services;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LivestreamApp.Tests.Unit.API;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authService = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authService.Object);
        // Setup HttpContext for cookie operations
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Register_Success_Returns200()
    {
        var user = User.Create("test@example.com", "hash");
        _authService.Setup(s => s.RegisterWithEmailAsync("test@example.com", "Password1!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.Register(new RegisterRequest("test@example.com", "Password1!"), default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsDomainException()
    {
        _authService.Setup(s => s.RegisterWithEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainException("An account with this email already exists."));

        var act = () => _sut.Register(new RegisterRequest("test@example.com", "Password1!"), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task Login_Success_SetsCookies()
    {
        var tokens = new AuthTokens("access-token", "refresh-token");
        _authService.Setup(s => s.LoginWithEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        var result = await _sut.Login(new LoginRequest("test@example.com", "Password1!"), default);

        result.Should().BeOfType<OkObjectResult>();
        _sut.Response.Cookies.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_InvalidPassword_ThrowsUnauthorized()
    {
        _authService.Setup(s => s.LoginWithEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedException("Invalid email or password."));

        var act = () => _sut.Login(new LoginRequest("test@example.com", "WrongPass!"), default);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task Refresh_Success_RotatesToken()
    {
        var tokens = new AuthTokens("new-access", "new-refresh");
        _sut.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refresh_token=old-token";
        _authService.Setup(s => s.RefreshAccessTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        var result = await _sut.Refresh(default);

        result.Should().BeOfType<OkObjectResult>();
    }
}
