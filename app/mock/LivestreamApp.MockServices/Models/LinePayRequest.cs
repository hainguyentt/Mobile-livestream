namespace LivestreamApp.MockServices.Models;

public class LinePayRequest
{
    public long Amount { get; set; }
    public string Currency { get; set; } = "JPY";
    public string OrderId { get; set; } = string.Empty;
    public List<LinePayProduct> Packages { get; set; } = [];
    public LinePayRedirectUrls RedirectUrls { get; set; } = new();
}

public class LinePayProduct
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long Amount { get; set; }
    public int Quantity { get; set; } = 1;
}

public class LinePayRedirectUrls
{
    public string ConfirmUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
