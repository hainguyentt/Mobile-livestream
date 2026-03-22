using FluentAssertions;
using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Auth.Options;
using LivestreamApp.Auth.Repositories;
using LivestreamApp.Auth.Services;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace LivestreamApp.Tests.Unit.Auth;

public class LineOAuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private LineOAuthService CreateSut(HttpClient httpClient)
    {
        var options = Options.Create(new LineOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            RedirectUri = "https://example.com/callback"
        });
        return new LineOAuthService(httpClient, options, _userRepo.Object, _uow.Object);
    }

    [Fact]
    public async Task LinkOrMergeAccount_NewUser_CreatesAccount()
    {
        _userRepo.Setup(r => r.GetByExternalLoginAsync("LINE", "line-user-123", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default)).Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.AddExternalLoginAsync(It.IsAny<ExternalLogin>(), default)).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var sut = CreateSut(new HttpClient());
        var result = await sut.LinkOrMergeAccountAsync("line-user-123", "user@example.com", "Test User");

        result.Should().NotBeNull();
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
    }

    [Fact]
    public async Task LinkOrMergeAccount_ExistingEmail_MergesAccount()
    {
        var existingUser = User.CreateFromExternalLogin("user@example.com");
        _userRepo.Setup(r => r.GetByExternalLoginAsync("LINE", "line-user-123", default)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(existingUser);
        _userRepo.Setup(r => r.AddExternalLoginAsync(It.IsAny<ExternalLogin>(), default)).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var sut = CreateSut(new HttpClient());
        var result = await sut.LinkOrMergeAccountAsync("line-user-123", "user@example.com", "Test User");

        result.Should().Be(existingUser);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task LinkOrMergeAccount_ExistingLineId_ReturnsExistingUser()
    {
        var existingUser = User.CreateFromExternalLogin("user@example.com");
        _userRepo.Setup(r => r.GetByExternalLoginAsync("LINE", "line-user-123", default)).ReturnsAsync(existingUser);

        var sut = CreateSut(new HttpClient());
        var result = await sut.LinkOrMergeAccountAsync("line-user-123", "user@example.com", "Test User");

        result.Should().Be(existingUser);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
    }
}
