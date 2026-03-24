using LivestreamApp.DirectChat.Domain.Entities;
using LivestreamApp.DirectChat.Repositories;
using LivestreamApp.DirectChat.Services;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace LivestreamApp.Tests.Unit.DirectChat;

public sealed class DirectChatServiceTests
{
    private readonly IConversationRepository _conversations = Substitute.For<IConversationRepository>();
    private readonly IDirectMessageRepository _messages = Substitute.For<IDirectMessageRepository>();
    private readonly IBlockRepository _blocks = Substitute.For<IBlockRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly DirectChatService _sut;

    public DirectChatServiceTests()
    {
        _sut = new DirectChatService(_conversations, _messages, _blocks, _uow,
            NullLogger<DirectChatService>.Instance);
    }

    [Fact]
    public async Task SendMessage_WhenNotBlocked_SendsSuccessfully()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var conversation = Conversation.Create(viewerId, hostId);

        _conversations.GetByIdForUserAsync(conversation.Id, viewerId).Returns(conversation);
        _blocks.ExistsAsync(viewerId, hostId).Returns(false);
        _blocks.ExistsAsync(hostId, viewerId).Returns(false);

        var message = await _sut.SendMessageAsync(conversation.Id, viewerId, "Hello!");

        Assert.Equal(viewerId, message.SenderId);
        Assert.Equal("Hello!", message.Content);
        Assert.Equal(MessageType.Text, message.MessageType);
        await _messages.Received(1).AddAsync(Arg.Any<DirectMessage>());
    }

    [Fact]
    public async Task SendMessage_WhenBlocked_ThrowsDomainException()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var conversation = Conversation.Create(viewerId, hostId);

        _conversations.GetByIdForUserAsync(conversation.Id, viewerId).Returns(conversation);
        _blocks.ExistsAsync(viewerId, hostId).Returns(true);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.SendMessageAsync(conversation.Id, viewerId, "Hello!"));
    }

    [Fact]
    public async Task BlockUser_HidesConversationForBothParties()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var conversation = Conversation.Create(viewerId, hostId);

        _blocks.ExistsAsync(viewerId, hostId).Returns(false);
        _conversations.GetByParticipantsAsync(viewerId, hostId).Returns(conversation);

        await _sut.BlockUserAsync(viewerId, hostId);

        Assert.True(conversation.IsHiddenByViewer);
        Assert.True(conversation.IsHiddenByHost);
        await _blocks.Received(1).AddAsync(Arg.Any<Block>());
    }

    [Fact]
    public async Task GetOrCreateConversation_WhenBlocked_ThrowsDomainException()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();

        _blocks.ExistsAsync(viewerId, hostId).Returns(true);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.GetOrCreateConversationAsync(viewerId, hostId));
    }

    [Fact]
    public async Task GetOrCreateConversation_WhenExisting_ReturnsExisting()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var existing = Conversation.Create(viewerId, hostId);

        _blocks.ExistsAsync(viewerId, hostId).Returns(false);
        _blocks.ExistsAsync(hostId, viewerId).Returns(false);
        _conversations.GetByParticipantsAsync(viewerId, hostId).Returns(existing);

        var result = await _sut.GetOrCreateConversationAsync(viewerId, hostId);

        Assert.Equal(existing.Id, result.Id);
        await _conversations.DidNotReceive().AddAsync(Arg.Any<Conversation>());
    }
}
