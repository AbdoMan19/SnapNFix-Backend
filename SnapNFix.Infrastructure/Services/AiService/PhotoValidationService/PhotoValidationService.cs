using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
                image_url = "https://www.google.com/imgres?q=pothole&imgurl=https%3A%2F%2Fimages.squarespace-cdn.com%2Fcontent%2Fv1%2F573365789f726693272dc91a%2F1704992146415-CI272VYXPALWT52IGLUB%2FAdobeStock_201419293.jpeg%3Fformat%3D1500w&imgrefurl=https%3A%2F%2Fwww.omag.org%2Fnews%2F2024%2F1%2F1%2Fpotholes-how-they-form-and-how-they-can-be-prevented&docid=gzLygswCYinemM&tbnid=bAgXLflP_xF8eM&vet=12ahUKEwjYt-GJk5qNAxXzKvsDHcfJKsAQM3oECGcQAA..i&w=1500&h=1004&hcb=2&ved=2ahUKEwjYt-GJk5qNAxXzKvsDHcfJKsAQM3oECGcQAA",
                webhook_url = "https://webhook.site/1a333a8c-54ce-492d-bb36-24a7aa3e4478"
            };

            using var httpClient = _httpClientFactory.CreateClient();
            using var formData = new MultipartFormDataContent();
            // Add form fields
            formData.Add(new StringContent(request.image_url), "image_url");
            formData.Add(new StringContent(request.webhook_url), "callback_url");
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
    private class ValidationResponse
    {
        public string task_id { get; set; }
        public string status { get; set; }
    }
}