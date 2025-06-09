using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Infrastructure.Options;

namespace SnapNFix.Infrastructure.Services.AiService.PhotoValidationService;

public class PhotoValidationService : IPhotoValidationService
{
    private readonly ILogger<PhotoValidationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PhotoValidationOptions _photoValidationOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PhotoValidationService(
        ILogger<PhotoValidationService> logger, 
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork,
        IOptions<PhotoValidationOptions> photoValidationOptions
        , IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _unitOfWork = unitOfWork;
        _photoValidationOptions = photoValidationOptions.Value;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<string> SendImageForValidationAsync(string imageUrl, CancellationToken cancellationToken)
    {
        try
        {
            var aiEndpoint = _photoValidationOptions.ValidationEndpoint;
            var webhookUrl = _photoValidationOptions.WebhookUrl;



            using var httpClient = _httpClientFactory.CreateClient();
            using var formData = new MultipartFormDataContent();
            
            formData.Add(new StringContent(imageUrl), "image_url");
            formData.Add(new StringContent(webhookUrl), "callback_url");
            
            var response = await httpClient.PostAsync(aiEndpoint, formData, cancellationToken);
            
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Image sent for validation. Status code: {StatusCode}", response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Response from AI service: {ResponseContent}", responseContent);
            
            var result = JsonConvert.DeserializeObject<ValidationResponse>(responseContent);

            if (result == null)
            {
                throw new Exception("Failed to deserialize response");
            }

            _logger.LogInformation("Image sent for validation. TaskId: {TaskId}, Status: {Status}", result.task_id, result.status);
            var taskId = result.task_id;
            return taskId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending image for validation");
            throw;
        }
    }

    public async Task ProcessPhotoValidationInBackgroundAsync(SnapReport snapReport)
    {
        _ = Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var taskId = await SendImageForValidationAsync(
                    snapReport.ImagePath,
                    CancellationToken.None);

                _logger.LogInformation("Image of report {ReportId} sent for validation. TaskId: {TaskId}",
                    snapReport.Id, taskId);

                snapReport.TaskId = taskId;
                await unitOfWork.Repository<SnapReport>().Update(snapReport);
                await unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing photo validation for report {ReportId}", snapReport.Id);
            }
            return Task.CompletedTask;
        });
    }
    private class ValidationResponse
    {
        public string task_id { get; set; }
        public string status { get; set; }
    }
}