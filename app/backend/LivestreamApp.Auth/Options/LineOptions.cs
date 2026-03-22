namespace LivestreamApp.Auth.Options;

public class LineOptions
{
    public const string SectionName = "Line";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
