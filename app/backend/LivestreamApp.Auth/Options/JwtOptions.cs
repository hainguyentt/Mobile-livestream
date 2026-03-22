namespace LivestreamApp.Auth.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 30;
}
