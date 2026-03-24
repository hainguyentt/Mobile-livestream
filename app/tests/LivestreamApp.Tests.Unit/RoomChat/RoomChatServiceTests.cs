using LivestreamApp.RoomChat.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace LivestreamApp.Tests.Unit.RoomChat;

public sealed class RoomChatServiceTests
{
    private readonly IConnectionMultiplexer _redis = Substitute.For<IConnectionMultiplexer>();
    private readonly IDatabase _db = Substitute.For<IDatabase>();
    private readonly IChatRateLimitService _rateLimit = Substitute.For<IChatRateLimitService>();
    private readonly RoomChatService _sut;

    public RoomChatServiceTests()
    {
        _redis.GetDatabase().Returns(_db);
        _sut = new RoomChatService(_redis, _rateLimit, NullLogger<RoomChatService>.Instance);
    }

    [Fact]
    public async Task SendMessage_WhenRateLimitOk_AddsToStream()
    {
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        _rateLimit.TryAcquire(senderId, roomId).Returns(true);
        _db.StreamAddAsync(Arg.Any<RedisKey>(), Arg.Any<NameValueEntry[]>(),
            maxLength: Arg.Any<int?>(), useApproximateMaxLength: Arg.Any<bool>(),
            flags: Arg.Any<CommandFlags>())
            .Returns(new RedisValue("1711029600000-0"));

        var message = await _sut.SendMessageAsync(roomId, senderId, "TestUser", "Hello!");

        Assert.Equal(roomId, message.RoomId);
        Assert.Equal(senderId, message.SenderId);
        Assert.Equal("Hello!", message.Content);
        Assert.Equal("message", message.Type);
    }

    [Fact]
    public async Task SendMessage_WhenRateLimitExceeded_ThrowsInvalidOperationException()
    {
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        _rateLimit.TryAcquire(senderId, roomId).Returns(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SendMessageAsync(roomId, senderId, "TestUser", "Spam!"));
    }

    [Fact]
    public async Task SendMessage_WhenContentExceedsMaxLength_TruncatesTo200Chars()
    {
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var longContent = new string('A', 300);
        _rateLimit.TryAcquire(senderId, roomId).Returns(true);
        _db.StreamAddAsync(Arg.Any<RedisKey>(), Arg.Any<NameValueEntry[]>(),
            maxLength: Arg.Any<int?>(), useApproximateMaxLength: Arg.Any<bool>(),
            flags: Arg.Any<CommandFlags>())
            .Returns(new RedisValue("1711029600000-0"));

        var message = await _sut.SendMessageAsync(roomId, senderId, "TestUser", longContent);

        Assert.Equal(200, message.Content.Length);
    }

    [Fact]
    public async Task SendGift_AddsGiftEventToStream()
    {
        var roomId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        _db.StreamAddAsync(Arg.Any<RedisKey>(), Arg.Any<NameValueEntry[]>(),
            maxLength: Arg.Any<int?>(), useApproximateMaxLength: Arg.Any<bool>(),
            flags: Arg.Any<CommandFlags>())
            .Returns(new RedisValue("1711029600000-0"));

        var message = await _sut.SendGiftAsync(roomId, senderId, "TestUser", "heart");

        Assert.Equal("gift", message.Type);
        Assert.Equal("heart", message.GiftId);
    }
}
