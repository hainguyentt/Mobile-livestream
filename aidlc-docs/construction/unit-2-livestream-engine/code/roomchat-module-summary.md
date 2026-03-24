# RoomChat Module Summary — Unit 2

**Module**: `LivestreamApp.RoomChat`  
**Ngày tạo**: 2026-03-22

## Phạm vi

Module xử lý room chat trong livestream — append-only Redis Streams, rate limiting, S3 archive.

## Cấu trúc

```
LivestreamApp.RoomChat/
├── Domain/
│   └── RoomChatMessage.cs          # Record — Redis Stream entry (không có DB table)
├── Services/
│   ├── IRoomChatService.cs / RoomChatService.cs
│   ├── IChatRateLimitService.cs / ChatRateLimitService.cs
└── Jobs/
    └── ExportRoomChatToS3Job.cs
```

## Các quyết định thiết kế chính

- Storage: Redis Stream `room:{roomId}:chat`, MAXLEN ~ 1000, TTL 7 ngày
- Rate limiting: in-memory `SlidingWindowRateLimiter` (3 msg/s per user per room)
- Gift events: lưu vào cùng stream với `type=gift`
- S3 export: JSON Lines format, key `chat-archive/{year}/{month}/{roomId}/{date}.jsonl`
- Dead Letter Queue: Hangfire retry policy cho export failures

## Stories được implement

- US-03-04: Chat trong phòng livestream
- US-03-05: Gửi gift cho Host
