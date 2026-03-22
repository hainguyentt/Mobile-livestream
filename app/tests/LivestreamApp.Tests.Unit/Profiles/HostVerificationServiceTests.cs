using FluentAssertions;
using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using LivestreamApp.Profiles.Services;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Moq;

namespace LivestreamApp.Tests.Unit.Profiles;

public class HostVerificationServiceTests
{
    private readonly Mock<IHostProfileRepository> _hostProfileRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly HostVerificationService _sut;

    public HostVerificationServiceTests()
    {
        _sut = new HostVerificationService(_hostProfileRepo.Object, _uow.Object);
    }

    [Fact]
    public async Task RequestVerification_NewProfile_CreatesAndSubmitsRequest()
    {
        var userId = Guid.NewGuid();
        _hostProfileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((HostProfile?)null);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.RequestVerificationAsync(userId);

        result.VerificationStatus.Should().Be(VerificationStatus.Pending);
        _hostProfileRepo.Verify(r => r.AddAsync(It.IsAny<HostProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestVerification_ExistingProfile_SubmitsRequest()
    {
        var userId = Guid.NewGuid();
        var existing = HostProfile.Create(userId);
        _hostProfileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.RequestVerificationAsync(userId);

        result.VerificationStatus.Should().Be(VerificationStatus.Pending);
        _hostProfileRepo.Verify(r => r.AddAsync(It.IsAny<HostProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveVerification_Success_GrantsBadgeAndEmitsEvent()
    {
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var hostProfile = HostProfile.Create(userId);
        hostProfile.RequestVerification();
        _hostProfileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(hostProfile);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ApproveVerificationAsync(userId, adminId);

        result.IsVerified.Should().BeTrue();
        result.VerificationStatus.Should().Be(VerificationStatus.Approved);
        result.VerifiedByAdminId.Should().Be(adminId);
        result.DomainEvents.Should().ContainSingle(e => e.GetType().Name == "HostVerifiedEvent");
    }

    [Fact]
    public async Task ApproveVerification_ProfileNotFound_ThrowsNotFoundException()
    {
        _hostProfileRepo.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((HostProfile?)null);

        var act = () => _sut.ApproveVerificationAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RejectVerification_Success_SetsRejectedStatus()
    {
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var hostProfile = HostProfile.Create(userId);
        hostProfile.RequestVerification();
        _hostProfileRepo.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(hostProfile);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.RejectVerificationAsync(userId, adminId, "Insufficient documentation.");

        result.VerificationStatus.Should().Be(VerificationStatus.Rejected);
        result.VerificationNote.Should().Be("Insufficient documentation.");
    }
}
