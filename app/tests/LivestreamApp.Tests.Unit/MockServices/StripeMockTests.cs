using FluentAssertions;
using LivestreamApp.MockServices.Controllers;
using LivestreamApp.MockServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace LivestreamApp.Tests.Unit.MockServices;

public class StripeMockTests
{
    private readonly StripeMockController _sut = new();

    [Fact]
    public void CreatePaymentIntent_Success_ReturnsPaymentIntentWithClientSecret()
    {
        // Arrange
        var request = new CreatePaymentIntentRequest(1000, "jpy", null);

        // Act
        var result = _sut.CreatePaymentIntent(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var intent = result!.Value as StripePaymentIntent;
        intent.Should().NotBeNull();
        intent!.Amount.Should().Be(1000);
        intent.Currency.Should().Be("jpy");
        intent.Status.Should().Be("requires_payment_method");
        intent.ClientSecret.Should().NotBeNullOrEmpty();
        intent.Id.Should().StartWith("pi_");
    }

    [Fact]
    public void ConfirmPaymentIntent_Success_ReturnsSucceededStatus()
    {
        // Arrange
        var createRequest = new CreatePaymentIntentRequest(2000, "jpy", null);
        var createResult = _sut.CreatePaymentIntent(createRequest) as OkObjectResult;
        var intent = createResult!.Value as StripePaymentIntent;
        var confirmRequest = new ConfirmPaymentIntentRequest("pm_card_visa");

        // Act
        var result = _sut.ConfirmPaymentIntent(intent!.Id, confirmRequest) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var confirmed = result!.Value as StripePaymentIntent;
        confirmed!.Status.Should().Be("succeeded");
        confirmed.PaymentMethod.Should().Be("pm_card_visa");
    }

    [Fact]
    public void ConfirmPaymentIntent_NotFound_Returns404()
    {
        // Arrange
        var request = new ConfirmPaymentIntentRequest(null);

        // Act
        var result = _sut.ConfirmPaymentIntent("pi_nonexistent", request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void TriggerWebhook_Success_ReturnsWebhookEvent()
    {
        // Arrange
        var createRequest = new CreatePaymentIntentRequest(3000, "jpy", null);
        var createResult = _sut.CreatePaymentIntent(createRequest) as OkObjectResult;
        var intent = createResult!.Value as StripePaymentIntent;
        var webhookRequest = new TriggerWebhookRequest(intent!.Id, "payment_intent.succeeded");

        // Act
        var result = _sut.TriggerWebhook(webhookRequest) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var webhookEvent = result!.Value as StripeWebhookEvent;
        webhookEvent!.Type.Should().Be("payment_intent.succeeded");
        webhookEvent.Id.Should().StartWith("evt_");
    }
}
