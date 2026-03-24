using LivestreamApp.API.DTOs.DirectChat;
using LivestreamApp.DirectChat.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LivestreamApp.API.Controllers.V1;

[ApiController]
[Route("api/v1/direct-chat")]
[Authorize]
public sealed class DirectChatController : ControllerBase
{
    private readonly IDirectChatService _chat;

    public DirectChatController(IDirectChatService chat)
    {
        _chat = chat;
    }

    /// <summary>GET /api/v1/direct-chat/conversations — List user's visible conversations.</summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(CancellationToken ct)
    {
        var userId = GetUserId();
        var conversations = await _chat.GetConversationsAsync(userId, ct);
        return Ok(conversations.Select(c => ConversationResponse.From(c, userId)));
    }

    /// <summary>GET /api/v1/direct-chat/conversations/{id} — Get conversation details.</summary>
    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> GetConversation(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var conversation = await _chat.GetConversationByIdAsync(id, userId, ct);
        if (conversation == null) return NotFound();
        return Ok(ConversationResponse.From(conversation, userId));
    }

    /// <summary>GET /api/v1/direct-chat/conversations/{id}/messages — Get messages (requires 'from' param).</summary>
    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, [FromQuery] GetMessagesQuery query, CancellationToken ct)
    {
        var userId = GetUserId();
        var messages = await _chat.GetMessagesAsync(id, userId, query.From, query.To, ct);
        return Ok(messages.Select(m => new
        {
            messageId = m.Id,
            senderId = m.SenderId,
            content = m.Content,
            messageType = m.MessageType.ToString(),
            emojiCode = m.EmojiCode,
            isRead = m.IsRead,
            sentAt = m.SentAt
        }));
    }

    /// <summary>POST /api/v1/direct-chat/conversations/{id}/read — Mark conversation as read.</summary>
    [HttpPost("conversations/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        await _chat.MarkAsReadAsync(id, GetUserId(), ct);
        return NoContent();
    }

    /// <summary>POST /api/v1/direct-chat/block/{userId} — Block a user.</summary>
    [HttpPost("block/{userId:guid}")]
    public async Task<IActionResult> BlockUser(Guid userId, CancellationToken ct)
    {
        await _chat.BlockUserAsync(GetUserId(), userId, ct);
        return NoContent();
    }

    /// <summary>DELETE /api/v1/direct-chat/block/{userId} — Unblock a user.</summary>
    [HttpDelete("block/{userId:guid}")]
    public async Task<IActionResult> UnblockUser(Guid userId, CancellationToken ct)
    {
        await _chat.UnblockUserAsync(GetUserId(), userId, ct);
        return NoContent();
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
