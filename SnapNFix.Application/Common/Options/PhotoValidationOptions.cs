namespace SnapNFix.Infrastructure.Options;

public class PhotoValidationOptions
{
    public string ValidationEndpoint { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public string? WebhookApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 120;
    public double DefaultApprovalThreshold { get; set; } = 0.3;
}