using MediatR;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;
using SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing.Dto_s;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public AiWebhookController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("validation-result")]
    public async Task<IActionResult> ReceiveValidationResult([FromBody] AiValidationWebhookDto request)
    {
        // Verify API key
        if (request.ApiKey != _configuration["AI:WebhookApiKey"])
        {
            return Unauthorized();
        }

        var command = new ImageValidationResultCommand
        {
            TaskId = request.TaskId,
            ImageStatus = request.ImageStatus,
            ReportCategory = request.Category,
            Threshold = request.Threshold
        };

        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
}