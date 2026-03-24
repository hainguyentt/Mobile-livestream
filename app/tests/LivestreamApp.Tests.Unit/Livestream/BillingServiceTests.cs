using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Livestream.Services;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace LivestreamApp.Tests.Unit.Livestream;

public sealed class BillingServiceTests
{
    private readonly ICallSessionRepository _callRepo = Substitute.For<ICallSessionRepository>();
    private readonly IBillingTickRepository _billingTicks = Substitute.For<IBillingTickRepository>();
    private readonly ICoinWalletService _wallet = Substitute.For<ICoinWalletService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly BillingService _sut;

    public BillingServiceTests()
    {
        _sut = new BillingService(_callRepo, _billingTicks, _wallet, _uow,
            NullLogger<BillingService>.Instance);
    }

    private static CallSession CreateActiveSession(int coinRate = 10)
    {
        var request = PrivateCallRequest.Create(Guid.NewGuid(), Guid.NewGuid(), coinRate);
        request.Accept();
        return CallSession.Create(request.Id, request.ViewerId, request.HostId, coinRate);
    }

    [Fact]
    public async Task ProcessBillingTick_WhenSufficientBalance_DeductsAndReturnsSuccess()
    {
        var session = CreateActiveSession(10);
        _callRepo.GetByIdAsync(session.Id).Returns(session);
        _wallet.GetBalanceAsync(session.ViewerId).Returns(100);
        _billingTicks.TryInsertAsync(Arg.Any<BillingTick>()).Returns(true);

        var result = await _sut.ProcessBillingTickAsync(session.Id, tickNumber: 1);

        Assert.Equal(BillingTickResult.Success, result);
        await _wallet.Received(1).DeductAsync(session.ViewerId, 10);
        Assert.Equal(10, session.TotalCoinsCharged);
    }

    [Fact]
    public async Task ProcessBillingTick_WhenDuplicateTick_ReturnsDuplicate()
    {
        var session = CreateActiveSession(10);
        _callRepo.GetByIdAsync(session.Id).Returns(session);
        _wallet.GetBalanceAsync(session.ViewerId).Returns(100);
        _billingTicks.TryInsertAsync(Arg.Any<BillingTick>()).Returns(false); // Duplicate

        var result = await _sut.ProcessBillingTickAsync(session.Id, tickNumber: 1);

        Assert.Equal(BillingTickResult.Duplicate, result);
        await _wallet.DidNotReceive().DeductAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ProcessBillingTick_WhenInsufficientBalance_ReturnsInsufficientBalance()
    {
        var session = CreateActiveSession(10);
        _callRepo.GetByIdAsync(session.Id).Returns(session);
        _wallet.GetBalanceAsync(session.ViewerId).Returns(5); // Less than 10

        var result = await _sut.ProcessBillingTickAsync(session.Id, tickNumber: 1);

        Assert.Equal(BillingTickResult.InsufficientBalance, result);
        await _billingTicks.DidNotReceive().TryInsertAsync(Arg.Any<BillingTick>());
    }

    [Fact]
    public async Task ProcessBillingTick_WhenSessionEnded_ReturnsSessionEnded()
    {
        var session = CreateActiveSession(10);
        session.End("Viewer");
        _callRepo.GetByIdAsync(session.Id).Returns(session);

        var result = await _sut.ProcessBillingTickAsync(session.Id, tickNumber: 1);

        Assert.Equal(BillingTickResult.SessionEnded, result);
    }

    [Fact]
    public async Task ProcessBillingTick_WhenSessionNotFound_ReturnsSessionEnded()
    {
        _callRepo.GetByIdAsync(Arg.Any<Guid>()).Returns((CallSession?)null);

        var result = await _sut.ProcessBillingTickAsync(Guid.NewGuid(), tickNumber: 1);

        Assert.Equal(BillingTickResult.SessionEnded, result);
    }
}
