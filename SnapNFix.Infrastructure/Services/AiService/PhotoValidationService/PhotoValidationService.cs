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
        IOptions<PhotoValidationOptions> photoValidationOptions,
        IServiceScopeFactory serviceScopeFactory)
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

            _logger.LogInformation("Sending image for validation to AI service. ImageUrl: {ImageUrl}, WebhookUrl: {WebhookUrl}", 
                imageUrl, webhookUrl);

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(_photoValidationOptions.TimeoutSeconds);
            
            // Add API key header if configured
            if (!string.IsNullOrEmpty(_photoValidationOptions.WebhookApiKey))
            {
                httpClient.DefaultRequestHeaders.Add("X-API-Key", _photoValidationOptions.WebhookApiKey);
            }
            
            using var formData = new MultipartFormDataContent();
            
            formData.Add(new StringContent(imageUrl), "image_url");
            formData.Add(new StringContent(webhookUrl), "callback_url");
            
            var response = await httpClient.PostAsync(aiEndpoint, formData, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI service returned error. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"AI service returned {response.StatusCode}: {errorContent}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Response from AI service: {ResponseContent}", responseContent);
            
            var result = JsonConvert.DeserializeObject<ValidationResponse>(responseContent);

            if (result == null || string.IsNullOrEmpty(result.task_id))
            {
                _logger.LogError("Invalid response from AI service: {ResponseContent}", responseContent);
                throw new InvalidOperationException("Failed to get valid task ID from AI service response");
            }

            _logger.LogInformation("Image sent for validation successfully. TaskId: {TaskId}, Status: {Status}", 
                result.task_id, result.status);
            
            return result.task_id;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error sending image for validation to AI service");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout sending image for validation to AI service");
            throw new TimeoutException("AI service request timed out", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending image for validation");
            throw;
        }
    }

    public async Task ProcessPhotoValidationInBackgroundAsync(SnapReport snapReport)
    {
        // Fire and forget background task
        _ = Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<PhotoValidationService>>();

            try
            {
                logger.LogInformation("Starting background photo validation for report {ReportId}", snapReport.Id);

                var taskId = await SendImageForValidationAsync(
                    snapReport.ImagePath, // This should be the full URL from Azure Blob Storage
                    CancellationToken.None);

                logger.LogInformation("Image of report {ReportId} sent for validation. TaskId: {TaskId}",
                    snapReport.Id, taskId);

                await using var transaction = await unitOfWork.BeginTransactionAsync();
                try
                {
                    var reportToUpdate = await unitOfWork.Repository<SnapReport>()
                        .FirstOrDefault(r => r.Id == snapReport.Id);
                    
                    if (reportToUpdate != null)
                    {
                        reportToUpdate.TaskId = taskId;
                        await unitOfWork.Repository<SnapReport>().Update(reportToUpdate);
                        await unitOfWork.SaveChanges();
                        await transaction.CommitAsync();
                        
                        logger.LogInformation("Successfully updated report {ReportId} with TaskId: {TaskId}", 
                            snapReport.Id, taskId);
                    }
                    else
                    {
                        logger.LogWarning("Report {ReportId} not found when trying to update TaskId", snapReport.Id);
                        await transaction.RollbackAsync();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    logger.LogError(ex, "Error updating report {ReportId} with TaskId: {TaskId}", 
                        snapReport.Id, taskId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing photo validation for report {ReportId}", snapReport.Id);
                
                // Mark the report as failed if validation service is unavailable
                try
                {
                    using var fallbackScope = _serviceScopeFactory.CreateScope();
                    var fallbackUnitOfWork = fallbackScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    
                    var reportToUpdate = await fallbackUnitOfWork.Repository<SnapReport>()
                        .FirstOrDefault(r => r.Id == snapReport.Id);
                    
                    if (reportToUpdate != null)
                    {
                        reportToUpdate.ImageStatus = Domain.Enums.ImageStatus.Declined;
                        await fallbackUnitOfWork.Repository<SnapReport>().Update(reportToUpdate);
                        await fallbackUnitOfWork.SaveChanges();
                        
                        logger.LogInformation("Marked report {ReportId} as declined due to validation error", snapReport.Id);
                    }
                }
                catch (Exception fallbackEx)
                {
                    logger.LogError(fallbackEx, "Failed to mark report {ReportId} as declined after validation error", 
                        snapReport.Id);
                }
            }
        });
    }
    
    private class ValidationResponse
    {
        public string task_id { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string? message { get; set; }
    }
}