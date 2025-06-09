namespace SnapNFix.Infrastructure.Options;

public class PhotoValidationOptions
{
    public string ValidationEndpoint { get; set; } = "snapnfix-ai-api.azurewebsites.net/api/v1/tasks/";
    public string WebhookUrl { get; set; } = "https://webhook.site/0b560717-3b92-4faa-9410-6cae643fbf0f";
}