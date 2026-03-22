using FluentAssertions;
using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using LivestreamApp.Profiles.Services;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Moq;

namespace LivestreamApp.Tests.Unit.Profiles;

public class PhotoServiceTests
{
    private readonly Mock<IPhotoRepository> _photoRepo = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly PhotoService _sut;

    public PhotoServiceTests()
    {
        _sut = new PhotoService(_photoRepo.Object, _storage.Object, _uow.Object, _cache.Object);
    }

    [Fact]
    public async Task GeneratePresignedUploadUrl_Success_ReturnsUrlAndPhotoId()
    {
        var userId = Guid.NewGuid();
        _storage.Setup(s => s.GeneratePresignedUploadUrlAsync(It.IsAny<string>(), "image/jpeg", 5_000_000, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://s3.example.com/presigned-url");

        var (url, photoId) = await _sut.GeneratePresignedUploadUrlAsync(userId, 0, "image/jpeg", 5_000_000);

        url.Should().Be("https://s3.example.com/presigned-url");
        photoId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GeneratePresignedUploadUrl_InvalidIndex_ThrowsDomainException()
    {
        var act = () => _sut.GeneratePresignedUploadUrlAsync(Guid.NewGuid(), 6, "image/jpeg", 1000);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Display index*");
    }

    [Fact]
    public async Task ConfirmPhotoUpload_Success_ReturnsPhoto()
    {
        var userId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var s3Key = $"photos/{userId}/{photoId}";
        _storage.Setup(s => s.ObjectExistsAsync(s3Key, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _photoRepo.Setup(r => r.GetByUserIdAndIndexAsync(userId, 0, It.IsAny<CancellationToken>())).ReturnsAsync((UserPhoto?)null);
        _photoRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ConfirmPhotoUploadAsync(userId, photoId, 0, s3Key, "https://cdn.example.com/photo.jpg", 100_000, "image/jpeg");

        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.DisplayIndex.Should().Be(0);
        _photoRepo.Verify(r => r.AddAsync(It.IsAny<UserPhoto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmPhotoUpload_ObjectNotFound_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var s3Key = $"photos/{userId}/{photoId}";
        _storage.Setup(s => s.ObjectExistsAsync(s3Key, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var act = () => _sut.ConfirmPhotoUploadAsync(userId, photoId, 0, s3Key, "https://cdn.example.com/photo.jpg", 100_000, "image/jpeg");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ConfirmPhotoUpload_ReplaceExistingAtIndex_DeletesOldPhoto()
    {
        var userId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var s3Key = $"photos/{userId}/{photoId}";
        var existingPhoto = UserPhoto.Create(userId, "photos/old-key", "https://cdn/old.jpg", 0, 500, "image/jpeg");

        _storage.Setup(s => s.ObjectExistsAsync(s3Key, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _photoRepo.Setup(r => r.GetByUserIdAndIndexAsync(userId, 0, It.IsAny<CancellationToken>())).ReturnsAsync(existingPhoto);
        _storage.Setup(s => s.DeleteObjectAsync("photos/old-key", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ConfirmPhotoUploadAsync(userId, photoId, 0, s3Key, "https://cdn.example.com/photo.jpg", 100_000, "image/jpeg");

        result.Should().NotBeNull();
        _storage.Verify(s => s.DeleteObjectAsync("photos/old-key", It.IsAny<CancellationToken>()), Times.Once);
        _photoRepo.Verify(r => r.Remove(existingPhoto), Times.Once);
    }

    [Fact]
    public async Task ReorderPhotos_Success_UpdatesDisplayIndices()
    {
        var userId = Guid.NewGuid();
        var photo0 = UserPhoto.Create(userId, "photos/0", "https://cdn/0.jpg", 0, 1000, "image/jpeg");
        var photo1 = UserPhoto.Create(userId, "photos/1", "https://cdn/1.jpg", 1, 1000, "image/jpeg");
        _photoRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([photo0, photo1]);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Swap order
        await _sut.ReorderPhotosAsync(userId, [photo1.Id, photo0.Id]);

        photo1.DisplayIndex.Should().Be(0);
        photo0.DisplayIndex.Should().Be(1);
    }

    [Fact]
    public async Task ReorderPhotos_MismatchedCount_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var photo0 = UserPhoto.Create(userId, "photos/0", "https://cdn/0.jpg", 0, 1000, "image/jpeg");
        _photoRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([photo0]);

        var act = () => _sut.ReorderPhotosAsync(userId, [Guid.NewGuid(), Guid.NewGuid()]);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*do not match*");
    }
}
