using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.AiService.PhotoValidationService;

public class PhotoValidationService : IPhotoValidationService
{
    private readonly ILogger<PhotoValidationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public PhotoValidationService( ILogger<PhotoValidationService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> SendImageForValidationAsync(string imagePath, CancellationToken cancellationToken)
    {
        try
        {
            var aiEndpoint = _configuration["AI:ValidationEndpoint"];
            var webhookUrl = _configuration["AI:WebhookUrl"];

            var request = new
            {
                ImageUrl = imagePath,
                WebhookUrl = webhookUrl
            };

            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync(aiEndpoint, request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var taskId = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Image sent for validation. TaskId: {TaskId}", taskId);

            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending image for validation");
            throw;
        }
    }
    
}