using LivestreamApp.MockServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace LivestreamApp.MockServices.Controllers;

/// <summary>Mock LINE Pay API for local development and testing.</summary>
[ApiController]
[Route("mock/linepay/v3/payments")]
public class LinePayMockController : ControllerBase
{
    private static readonly Dictionary<long, (LinePayRequest Request, string Status)> _transactions = [];
    private static long _transactionCounter = 1000000000L;

    /// <summary>Requests a LINE Pay payment — returns a mock payment URL.</summary>
    [HttpPost("request")]
    public IActionResult RequestPayment([FromBody] LinePayRequest request)
    {
        var transactionId = Interlocked.Increment(ref _transactionCounter);
        _transactions[transactionId] = (request, "AUTHORIZATION");

        var response = new LinePayResponse
        {
            Info = new LinePayResponseInfo
            {
                TransactionId = transactionId,
                OrderId = request.OrderId,
                PaymentUrl = new LinePayPaymentUrl
                {
                    Web = $"http://localhost:5200/mock/linepay/pay?transactionId={transactionId}",
                    App = $"linepay://pay?transactionId={transactionId}"
                }
            }
        };

        return Ok(response);
    }

    /// <summary>Confirms a LINE Pay transaction — simulates user completing payment.</summary>
    [HttpPost("{transactionId}/confirm")]
    public IActionResult ConfirmPayment(long transactionId, [FromBody] ConfirmLinePayRequest request)
    {
        if (!_transactions.TryGetValue(transactionId, out var txn))
            return NotFound(new LinePayResponse { ReturnCode = "1104", ReturnMessage = "Transaction not found." });

        _transactions[transactionId] = (txn.Request, "CAPTURE");

        var response = new LinePayResponse
        {
            Info = new LinePayResponseInfo
            {
                TransactionId = transactionId,
                OrderId = txn.Request.OrderId,
                TransactionStatus = "CAPTURE"
            }
        };

        return Ok(response);
    }

    /// <summary>Gets the status of a LINE Pay transaction.</summary>
    [HttpGet("{transactionId}")]
    public IActionResult GetPayment(long transactionId)
    {
        if (!_transactions.TryGetValue(transactionId, out var txn))
            return NotFound(new LinePayResponse { ReturnCode = "1104", ReturnMessage = "Transaction not found." });

        var response = new LinePayResponse
        {
            Info = new LinePayResponseInfo
            {
                TransactionId = transactionId,
                OrderId = txn.Request.OrderId,
                TransactionStatus = txn.Status
            }
        };

        return Ok(response);
    }
}

public record ConfirmLinePayRequest(long Amount, string Currency);
