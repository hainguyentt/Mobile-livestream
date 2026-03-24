using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamApp.API.Migrations
{
    /// <inheritdoc />
    public partial class Unit2LivestreamEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoinBalance",
                table: "user_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "call_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgoraChannelName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CoinRatePerTick = table.Column<int>(type: "integer", nullable: false),
                    TotalCoinsCharged = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalTicks = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedBy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_call_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastMessagePreview = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ViewerUnreadCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HostUnreadCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsHiddenByViewer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHiddenByHost = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "livestream_rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AgoraChannelName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ViewerCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PeakViewerCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalViewerCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_livestream_rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "private_call_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    HostId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CoinRatePerTick = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_private_call_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "billing_ticks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CallSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TickNumber = table.Column<int>(type: "integer", nullable: false),
                    CoinsCharged = table.Column<int>(type: "integer", nullable: false),
                    ViewerBalanceBefore = table.Column<int>(type: "integer", nullable: false),
                    ViewerBalanceAfter = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billing_ticks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_billing_ticks_call_sessions_CallSessionId",
                        column: x => x.CallSessionId,
                        principalTable: "call_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "direct_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    MessageType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmojiCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeletedBySender = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_direct_messages", x => new { x.Id, x.SentAt });
                    table.ForeignKey(
                        name: "FK_direct_messages_conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kicked_viewers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    KickedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    KickedByRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KickedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kicked_viewers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kicked_viewers_livestream_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "livestream_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "viewer_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WatchDurationSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsKicked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_viewer_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_viewer_sessions_livestream_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "livestream_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_billing_ticks_session_tick_unique",
                table: "billing_ticks",
                columns: new[] { "CallSessionId", "TickNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_blocks_blocked",
                table: "blocks",
                column: "BlockedId");

            migrationBuilder.CreateIndex(
                name: "idx_blocks_blocker",
                table: "blocks",
                column: "BlockerId");

            migrationBuilder.CreateIndex(
                name: "idx_blocks_blocker_blocked_unique",
                table: "blocks",
                columns: new[] { "BlockerId", "BlockedId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_call_sessions_request_unique",
                table: "call_sessions",
                column: "CallRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_conversations_host",
                table: "conversations",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "idx_conversations_viewer",
                table: "conversations",
                column: "ViewerId");

            migrationBuilder.CreateIndex(
                name: "idx_conversations_viewer_host_unique",
                table: "conversations",
                columns: new[] { "ViewerId", "HostId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_direct_messages_conversation_sent",
                table: "direct_messages",
                columns: new[] { "ConversationId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "idx_kicked_viewers_room_viewer_unique",
                table: "kicked_viewers",
                columns: new[] { "RoomId", "ViewerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_livestream_rooms_host_live_unique",
                table: "livestream_rooms",
                column: "HostId",
                unique: true,
                filter: "\"Status\" = 'Live'");

            migrationBuilder.CreateIndex(
                name: "idx_livestream_rooms_status",
                table: "livestream_rooms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_call_requests_host_pending_unique",
                table: "private_call_requests",
                column: "HostId",
                unique: true,
                filter: "\"Status\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "idx_call_requests_viewer",
                table: "private_call_requests",
                column: "ViewerId");

            migrationBuilder.CreateIndex(
                name: "idx_viewer_sessions_active_unique",
                table: "viewer_sessions",
                columns: new[] { "RoomId", "ViewerId" },
                unique: true,
                filter: "\"LeftAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_viewer_sessions_room",
                table: "viewer_sessions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "idx_viewer_sessions_viewer",
                table: "viewer_sessions",
                column: "ViewerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "billing_ticks");

            migrationBuilder.DropTable(
                name: "blocks");

            migrationBuilder.DropTable(
                name: "direct_messages");

            migrationBuilder.DropTable(
                name: "kicked_viewers");

            migrationBuilder.DropTable(
                name: "private_call_requests");

            migrationBuilder.DropTable(
                name: "viewer_sessions");

            migrationBuilder.DropTable(
                name: "call_sessions");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "livestream_rooms");

            migrationBuilder.DropColumn(
                name: "CoinBalance",
                table: "user_profiles");
        }
    }
}
