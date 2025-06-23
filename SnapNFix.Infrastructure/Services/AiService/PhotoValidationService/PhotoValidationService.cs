using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SnapNFix.Application.Interfaces;
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
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;

    public PhotoValidationService(
        ILogger<PhotoValidationService> logger, 
        IHttpClientFactory httpClientFactory,
        IUnitOfWork unitOfWork,
        IOptions<PhotoValidationOptions> photoValidationOptions,
        IBackgroundTaskQueue backgroundTaskQueue)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _unitOfWork = unitOfWork;
        _photoValidationOptions = photoValidationOptions.Value;
        _backgroundTaskQueue = backgroundTaskQueue;
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
        var reportId = snapReport.Id;
        var imageUrl = snapReport.ImagePath;
        
        _logger.LogInformation("Queueing background photo validation for report {ReportId}", reportId);
        
        // Queue the validation in the background task queue with retry logic
        _backgroundTaskQueue.Enqueue(async (serviceProvider, cancellationToken) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<PhotoValidationService>>();
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var options = serviceProvider.GetRequiredService<IOptions<PhotoValidationOptions>>();
            
            logger.LogInformation("Starting background photo validation for report {ReportId}", reportId);
            
            const int maxRetries = 3;
            int retryCount = 0;
            string? taskId = null;
            
            while (retryCount < maxRetries && taskId == null)
            {
                try
                {
                    // Create a validation service instance within the scope
                    var validationService = new PhotoValidationService(
                        logger,
                        httpClientFactory,
                        unitOfWork,
                        options,
                        serviceProvider.GetRequiredService<IBackgroundTaskQueue>());
                    
                    taskId = await validationService.SendImageForValidationAsync(
                        imageUrl, 
                        cancellationToken);
                        
                    break;
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TimeoutException)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        logger.LogError(ex, "Failed to send image for validation after {RetryCount} attempts for report {ReportId}", 
                            retryCount, reportId);
                        // Mark as failed after all retries
                        await MarkReportAsDeclined(unitOfWork, reportId, logger);
                        return;
                    }
                    
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount)); // Exponential backoff
                    logger.LogWarning(ex, "Error sending image for validation, retry {RetryCount}/{MaxRetries} in {Delay}s for report {ReportId}", 
                        retryCount, maxRetries, delay.TotalSeconds, reportId);
                    await Task.Delay(delay, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error processing photo validation for report {ReportId}", reportId);
                    await MarkReportAsDeclined(unitOfWork, reportId, logger);
                    return;
                }
            }
            
            if (taskId == null)
            {
                logger.LogError("Failed to get task ID for report {ReportId}", reportId);
                await MarkReportAsDeclined(unitOfWork, reportId, logger);
                return;
            }
            
            // Update report with task ID
            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var reportToUpdate = await unitOfWork.Repository<SnapReport>()
                    .FirstOrDefault(r => r.Id == reportId);
                
                if (reportToUpdate != null)
                {
                    reportToUpdate.TaskId = taskId;
                    await unitOfWork.Repository<SnapReport>().Update(reportToUpdate);
                    await unitOfWork.SaveChanges();
                    await transaction.CommitAsync(cancellationToken);
                    
                    logger.LogInformation("Successfully updated report {ReportId} with TaskId: {TaskId}", 
                        reportId, taskId);
                }
                else
                {
                    logger.LogWarning("Report {ReportId} not found when trying to update TaskId", reportId);
                    await transaction.RollbackAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                logger.LogError(ex, "Error updating report {ReportId} with TaskId: {TaskId}", 
                    reportId, taskId);
            }
        });
    }
    
    private static async Task MarkReportAsDeclined(IUnitOfWork unitOfWork, Guid reportId, ILogger logger)
    {
        try
        {
            await using var transaction = await unitOfWork.BeginTransactionAsync();
            
            var reportToUpdate = await unitOfWork.Repository<SnapReport>()
                .FirstOrDefault(r => r.Id == reportId);
            
            if (reportToUpdate != null)
            {
                reportToUpdate.ImageStatus = Domain.Enums.ImageStatus.Declined;
                await unitOfWork.Repository<SnapReport>().Update(reportToUpdate);
                await unitOfWork.SaveChanges();
                await transaction.CommitAsync();
                
                logger.LogInformation("Marked report {ReportId} as declined due to validation error", reportId);
            }
            else
            {
                logger.LogWarning("Report {ReportId} not found when trying to mark as declined", reportId);
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark report {ReportId} as declined after validation error", reportId);
        }
    }
    
    private class ValidationResponse
    {
        public string task_id { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string? message { get; set; }
    }
}
