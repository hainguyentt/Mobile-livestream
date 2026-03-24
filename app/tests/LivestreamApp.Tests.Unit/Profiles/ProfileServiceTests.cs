using FluentAssertions;
using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using LivestreamApp.Profiles.Services;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Moq;

namespace LivestreamApp.Tests.Unit.Profiles;

public class ProfileServiceTests
{
    private readonly Mock<IProfileRepository> _profileRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        _sut = new ProfileService(_profileRepo.Object, _uow.Object);
    }

    [Fact]
    public async Task CreateProfile_Success_ReturnsProfile()
    {
        var userId = Guid.NewGuid();
        _profileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile?)null);
        _profileRepo.Setup(r => r.IsDisplayNameTakenAsync("TestUser", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateProfileAsync(userId, "TestUser", new DateOnly(1990, 1, 1));

        result.Should().NotBeNull();
        result.DisplayName.Should().Be("TestUser");
        result.IsProfileComplete.Should().BeTrue();
        _profileRepo.Verify(r => r.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProfile_DuplicateDisplayName_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        _profileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile?)null);
        _profileRepo.Setup(r => r.IsDisplayNameTakenAsync("TakenName", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateProfileAsync(userId, "TakenName", new DateOnly(1990, 1, 1));

        await act.Should().ThrowAsync<DomainException>().WithMessage("*already taken*");
    }

    [Fact]
    public async Task CreateProfile_AlreadyExists_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var existing = UserProfile.Create(userId, "ExistingUser", new DateOnly(1990, 1, 1));
        _profileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var act = () => _sut.CreateProfileAsync(userId, "NewName", new DateOnly(1990, 1, 1));

        await act.Should().ThrowAsync<DomainException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task UpdateProfile_Success_UpdatesBioAndInterests()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, "TestUser", new DateOnly(1990, 1, 1));
        _profileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateProfileAsync(userId, "New bio", ["music", "travel"], null);

        result.Bio.Should().Be("New bio");
        result.Interests.Should().Contain("music");
    }

    [Fact]
    public async Task UpdateProfile_ProfileNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _profileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile?)null);

        var act = () => _sut.UpdateProfileAsync(userId, "bio", null, null);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetProfile_LoadsFromDb()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, "DbUser", new DateOnly(1990, 1, 1));
        _profileRepo.Setup(r => r.GetWithPhotosAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var result = await _sut.GetProfileAsync(userId);

        result.Should().Be(profile);
        _profileRepo.Verify(r => r.GetWithPhotosAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProfile_NotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        _profileRepo.Setup(r => r.GetWithPhotosAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserProfile?)null);

        var act = () => _sut.GetProfileAsync(userId);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
