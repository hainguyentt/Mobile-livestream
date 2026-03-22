namespace LivestreamApp.MockServices.Models;

public class StripePaymentIntent
{
    public string Id { get; set; } = $"pi_{Guid.NewGuid():N}";
    public string Object { get; set; } = "payment_intent";
    public long Amount { get; set; }
    public string Currency { get; set; } = "jpy";
    public string Status { get; set; } = "requires_payment_method";
    public string? ClientSecret { get; set; }
    public string? PaymentMethod { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
    public long Created { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
