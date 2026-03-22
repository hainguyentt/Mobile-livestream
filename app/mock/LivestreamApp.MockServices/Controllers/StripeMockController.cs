using LivestreamApp.MockServices.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace LivestreamApp.MockServices.Controllers;

/// <summary>Mock Stripe API for local development and testing.</summary>
[ApiController]
[Route("mock/stripe/v1")]
public class StripeMockController : ControllerBase
{
    // Thread-safe in-memory store for mock payment intents
    private static readonly ConcurrentDictionary<string, StripePaymentIntent> _intents = new();

    /// <summary>Creates a mock payment intent.</summary>
    [HttpPost("payment_intents")]
    public IActionResult CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
    {
        var intent = new StripePaymentIntent
        {
            Amount = request.Amount,
            Currency = request.Currency ?? "jpy",
            Status = "requires_payment_method",
            ClientSecret = $"pi_{Guid.NewGuid():N}_secret_{Guid.NewGuid():N}",
            Metadata = request.Metadata ?? []
        };

        _intents[intent.Id] = intent;
        return Ok(intent);
    }

    /// <summary>Confirms a mock payment intent — simulates successful payment.</summary>
    [HttpPost("payment_intents/{id}/confirm")]
    public IActionResult ConfirmPaymentIntent(string id, [FromBody] ConfirmPaymentIntentRequest request)
    {
        if (!_intents.TryGetValue(id, out var intent))
            return NotFound(new { error = new { message = $"No such payment_intent: '{id}'" } });

        intent.Status = "succeeded";
        intent.PaymentMethod = request.PaymentMethod ?? "pm_card_visa";
        return Ok(intent);
    }

    /// <summary>Manually triggers a webhook event for testing.</summary>
    [HttpPost("webhooks")]
    public IActionResult TriggerWebhook([FromBody] TriggerWebhookRequest request)
    {
        var paymentIntent = _intents.Values.FirstOrDefault(i => i.Id == request.PaymentIntentId);

        var webhookEvent = new StripeWebhookEvent
        {
            Type = request.EventType ?? "payment_intent.succeeded",
            Data = new StripeEventData { Object = paymentIntent }
        };

        return Ok(webhookEvent);
    }
}

public record CreatePaymentIntentRequest(long Amount, string? Currency, Dictionary<string, string>? Metadata);
public record ConfirmPaymentIntentRequest(string? PaymentMethod);
public record TriggerWebhookRequest(string? PaymentIntentId, string? EventType);
