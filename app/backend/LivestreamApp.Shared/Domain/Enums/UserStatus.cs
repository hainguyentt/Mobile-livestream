namespace LivestreamApp.Shared.Domain.Enums;

public enum UserStatus
{
    PendingVerification = 0,  // Chờ xác minh email
    Active = 1,               // Đang hoạt động
    Suspended = 2,            // Bị tạm khóa
    Banned = 3                // Bị cấm vĩnh viễn
}
