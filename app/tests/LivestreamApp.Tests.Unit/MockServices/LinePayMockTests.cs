using FluentAssertions;
using LivestreamApp.MockServices.Controllers;
using LivestreamApp.MockServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace LivestreamApp.Tests.Unit.MockServices;

public class LinePayMockTests
{
    private readonly LinePayMockController _sut = new();

    [Fact]
    public void RequestPayment_Success_ReturnsTransactionWithPaymentUrl()
    {
        // Arrange
        var request = new LinePayRequest
        {
            Amount = 1000,
            Currency = "JPY",
            OrderId = "order-001",
            Packages = [new LinePayProduct { Id = "pkg-1", Name = "Coins", Amount = 1000 }],
            RedirectUrls = new LinePayRedirectUrls
            {
                ConfirmUrl = "https://example.com/confirm",
                CancelUrl = "https://example.com/cancel"
            }
        };

        // Act
        var result = _sut.RequestPayment(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var response = result!.Value as LinePayResponse;
        response!.ReturnCode.Should().Be("0000");
        response.Info.Should().NotBeNull();
        response.Info!.TransactionId.Should().BeGreaterThan(0);
        response.Info.OrderId.Should().Be("order-001");
        response.Info.PaymentUrl.Should().NotBeNull();
        response.Info.PaymentUrl!.Web.Should().Contain("transactionId=");
    }

    [Fact]
    public void ConfirmPayment_Success_ReturnsCaptureStatus()
    {
        // Arrange
        var payRequest = new LinePayRequest
        {
            Amount = 2000,
            Currency = "JPY",
            OrderId = "order-002",
            Packages = [],
            RedirectUrls = new LinePayRedirectUrls()
        };
        var payResult = _sut.RequestPayment(payRequest) as OkObjectResult;
        var payResponse = payResult!.Value as LinePayResponse;
        var transactionId = payResponse!.Info!.TransactionId;
        var confirmRequest = new ConfirmLinePayRequest(2000, "JPY");

        // Act
        var result = _sut.ConfirmPayment(transactionId, confirmRequest) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var response = result!.Value as LinePayResponse;
        response!.ReturnCode.Should().Be("0000");
        response.Info!.TransactionStatus.Should().Be("CAPTURE");
    }

    [Fact]
    public void GetPayment_NotFound_ReturnsErrorCode()
    {
        // Act
        var result = _sut.GetPayment(9999999999L);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFound = result as NotFoundObjectResult;
        var response = notFound!.Value as LinePayResponse;
        response!.ReturnCode.Should().Be("1104");
    }
}
