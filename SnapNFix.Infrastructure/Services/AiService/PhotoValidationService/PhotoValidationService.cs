using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Infrastructure.Options;

namespace SnapNFix.Infrastructure.Services.AiService.PhotoValidationService;

public class PhotoValidationService : IPhotoValidationService
{
    private readonly ILogger<PhotoValidationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PhotoValidationOptions _photoValidationOptions;

    public PhotoValidationService(
        ILogger<PhotoValidationService> logger, 
        IConfiguration configuration, 
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork,
        IOptions<PhotoValidationOptions> photoValidationOptions)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _unitOfWork = unitOfWork;
        _photoValidationOptions = photoValidationOptions.Value;
    }

    public async Task<string> SendImageForValidationAsync(string imagePath, CancellationToken cancellationToken)
    {
        try
        {
            var aiEndpoint = _photoValidationOptions.ValidationEndpoint;
            var webhookUrl = _photoValidationOptions.WebhookUrl;

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

    public async Task ProcessPhotoValidationInBackgroundAsync(Domain.Entities.SnapReport snapReport)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                var taskId = await SendImageForValidationAsync(
                    snapReport.ImagePath,
                    CancellationToken.None);

                _logger.LogInformation("Image of report {ReportId} sent for validation. TaskId: {TaskId}", 
                    snapReport.Id, taskId);

                snapReport.TaskId = taskId;
                await _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing photo validation for report {ReportId}", snapReport.Id);
            }
        });
    }
}