namespace LivestreamApp.MockServices.Models;

public class LinePayResponse
{
    public string ReturnCode { get; set; } = "0000";
    public string ReturnMessage { get; set; } = "Success.";
    public LinePayResponseInfo? Info { get; set; }
}

public class LinePayResponseInfo
{
    public long TransactionId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public LinePayPaymentUrl? PaymentUrl { get; set; }
    public string? TransactionStatus { get; set; }
    public long? AuthorizationExpireDate { get; set; }
}

public class LinePayPaymentUrl
{
    public string Web { get; set; } = string.Empty;
    public string App { get; set; } = string.Empty;
}
