using FluentAssertions;
using LivestreamApp.API.Controllers.V1;
using LivestreamApp.API.DTOs.Profiles;
using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace LivestreamApp.Tests.Unit.API;

public class ProfilesControllerTests
{
    private readonly Mock<IProfileService> _profileService = new();
    private readonly Mock<IPhotoService> _photoService = new();
    private readonly ProfilesController _sut;
    private readonly Guid _userId = Guid.NewGuid();

    public ProfilesControllerTests()
    {
        _sut = new ProfilesController(_profileService.Object, _photoService.Object);

        // Setup authenticated user context
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task GetMyProfile_Success_Returns200()
    {
        var profile = UserProfile.Create(_userId, "TestUser", new DateOnly(1990, 1, 1));
        _profileService.Setup(s => s.GetProfileAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var result = await _sut.GetMyProfile(default);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(profile);
    }

    [Fact]
    public async Task UpdateProfile_Success_Returns200()
    {
        var profile = UserProfile.Create(_userId, "TestUser", new DateOnly(1990, 1, 1));
        _profileService.Setup(s => s.UpdateProfileAsync(_userId, "New bio", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _sut.UpdateProfile(new UpdateProfileRequest("New bio", null, null), default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task PresignPhoto_Success_ReturnsUploadUrl()
    {
        _photoService.Setup(s => s.GeneratePresignedUploadUrlAsync(_userId, 0, "image/jpeg", 1000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("https://s3.example.com/upload", Guid.NewGuid()));

        var result = await _sut.PresignPhoto(new PresignPhotoRequest(0, "image/jpeg", 1000), default);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ConfirmPhoto_Success_Returns201()
    {
        var photoId = Guid.NewGuid();
        var photo = UserPhoto.Create(_userId, "photos/key", "https://cdn/photo.jpg", 0, 1000, "image/jpeg");
        _photoService.Setup(s => s.ConfirmPhotoUploadAsync(_userId, photoId, It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var result = await _sut.ConfirmPhoto(new ConfirmPhotoRequest(photoId, 0, "photos/key", "https://cdn/photo.jpg", 1000, "image/jpeg"), default);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(201);
    }
}
