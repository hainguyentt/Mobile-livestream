namespace LivestreamApp.MockServices.Models;

public class StripeWebhookEvent
{
    public string Id { get; set; } = $"evt_{Guid.NewGuid():N}";
    public string Object { get; set; } = "event";
    public string Type { get; set; } = string.Empty;
    public StripeEventData Data { get; set; } = new();
    public long Created { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public bool Livemode { get; set; } = false;
}

public class StripeEventData
{
    public object? Object { get; set; }
}
