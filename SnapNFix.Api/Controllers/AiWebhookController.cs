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
        _logger.LogInformation("Received AI validation webhook for TaskId: {TaskId}, Status: {Status}, Category: {Category}, Threshold: {Threshold}", 
            request.TaskId, request.Status, request.Category, request.Threshold);

        // Verify API key if configured
        if (!string.IsNullOrEmpty(_photoValidationOptions.WebhookApiKey))
        {
            var providedApiKey = Request.Headers["SNAPNFIX_API_KEY"].FirstOrDefault();
            if (string.IsNullOrEmpty(providedApiKey) || providedApiKey != _photoValidationOptions.WebhookApiKey)
            {
                _logger.LogWarning("Unauthorized webhook attempt with invalid or missing API key for TaskId: {TaskId}", request.TaskId);
                return Unauthorized("Invalid or missing API key");
            }
        }

        if (string.IsNullOrEmpty(request.TaskId))
        {
            _logger.LogWarning("Webhook received without TaskId");
            return BadRequest("TaskId is required");
        }

        var command = new ImageValidationResultCommand
        {
            TaskId = request.TaskId,
            ImageStatus = request.ImageStatus,  
            ReportCategory = request.ReportCategory,  
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
}