using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;
using SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing.Dto_s;
using SnapNFix.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly PhotoValidationOptions _photoValidationOptions;
    private readonly ILogger<AiWebhookController> _logger;

    public AiWebhookController(
        IMediator mediator, 
        IConfiguration configuration, 
        IOptions<PhotoValidationOptions> photoValidationOptions,
        ILogger<AiWebhookController> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _photoValidationOptions = photoValidationOptions.Value;
        _logger = logger;
    }

    [HttpPost("validation-result")]
    public async Task<IActionResult> ReceiveValidationResult([FromBody] AiValidationWebhookDto request)
    {
        try
        {
            _logger.LogInformation("Received AI validation webhook for TaskId: {TaskId}, Status: {Status}, Category: {Category}, Threshold: {Threshold}", 
                request.TaskId, request.ImageStatus, request.Category, request.Threshold);

            // Optional: Verify API key if you want to secure the webhook
            // if (!string.IsNullOrEmpty(_photoValidationOptions.WebhookApiKey) && 
            //     request.ApiKey != _photoValidationOptions.WebhookApiKey)
            // {
            //     _logger.LogWarning("Unauthorized webhook attempt with invalid API key for TaskId: {TaskId}", request.TaskId);
            //     return Unauthorized("Invalid API key");
            // }

            if (string.IsNullOrEmpty(request.TaskId))
            {
                _logger.LogWarning("Webhook received without TaskId");
                return BadRequest("TaskId is required");
            }

            var imageStatus = MapToImageStatus(request.ImageStatus);
            var reportCategory = MapToReportCategory(request.ReportCategory);

            var command = new ImageValidationResultCommand
            {
                TaskId = request.TaskId,
                ImageStatus = imageStatus,
                ReportCategory = reportCategory,
                Threshold = request.Threshold
            };

            var result = await _mediator.Send(command);
            
            if (result.ErrorList.Count != 0)
            {
                _logger.LogError("Failed to process AI validation result for TaskId: {TaskId}. Errors: {Errors}", 
                    request.TaskId, string.Join(", ", result.ErrorList.Select(e => e.Message)));
                return BadRequest(result);
            }

            _logger.LogInformation("Successfully processed AI validation result for TaskId: {TaskId}", request.TaskId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing AI validation webhook for TaskId: {TaskId}", request?.TaskId);
            return StatusCode(500, "Internal server error while processing validation result");
        }
    }

    private ImageStatus MapToImageStatus(ImageStatus status)
    {
        return status switch
        {
            ImageStatus.Approved => ImageStatus.Approved,
            ImageStatus.Declined => ImageStatus.Declined,
            ImageStatus.Pending => ImageStatus.Pending,
            _ => ImageStatus.Pending
        };
    }

    private ReportCategory MapToReportCategory(ReportCategory category)
    {
        return category switch
        {
            ReportCategory.Garbage => ReportCategory.Garbage,
            ReportCategory.Pothole => ReportCategory.Pothole,
            ReportCategory.DefectiveManhole => ReportCategory.DefectiveManhole,
            ReportCategory.NonDefectiveManhole => ReportCategory.NonDefectiveManhole,
            _ => ReportCategory.NotSpecified
        };
    }
}