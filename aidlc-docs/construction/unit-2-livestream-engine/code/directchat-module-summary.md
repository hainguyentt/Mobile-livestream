# DirectChat Module Summary — Unit 2

**Module**: `LivestreamApp.DirectChat`  
**Ngày tạo**: 2026-03-22

## Phạm vi

Module xử lý 1-1 messaging giữa Viewer và Host, block/unblock, conversation management.

## Cấu trúc

```
LivestreamApp.DirectChat/
├── Domain/
│   └── Entities/
│       ├── Conversation.cs     # Aggregate root — 1 per Viewer-Host pair
│       ├── DirectMessage.cs    # Partitioned by sent_at (monthly)
│       └── Block.cs            # Block relationship
├── Repositories/
│   ├── IConversationRepository.cs
│   ├── IDirectMessageRepository.cs
│   └── IBlockRepository.cs
└── Services/
    ├── IDirectChatService.cs
    └── DirectChatService.cs
```

## Các quyết định thiết kế chính

- `direct_messages` table: PostgreSQL range partitioning by `sent_at` (monthly)
- `GetMessagesAsync`: `from` parameter bắt buộc — partition safeguard
- Block: ẩn conversation cả 2 phía (`IsHiddenByViewer = IsHiddenByHost = true`)
- Unique constraint: `(viewer_id, host_id)` trên `conversations`
- Retention: 12 tháng — Hangfire monthly job drop expired partitions

## Stories được implement

- US-05-01: Nhắn tin trực tiếp (DirectChat)
- US-05-02: Block / Unblock người dùng
