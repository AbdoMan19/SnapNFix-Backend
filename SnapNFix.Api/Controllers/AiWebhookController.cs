using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;
using SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing.Dto_s;
using SnapNFix.Infrastructure.Options;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly PhotoValidationOptions _photoValidationOptions;

    public AiWebhookController(IMediator mediator, IConfiguration configuration , IOptions<PhotoValidationOptions> photoValidationOptions)
    {
        _mediator = mediator;
        _configuration = configuration;
        _photoValidationOptions = photoValidationOptions.Value;
    }

    [HttpPost("validation-result")]
    public async Task<IActionResult> ReceiveValidationResult([FromBody] AiValidationWebhookDto request)
    {
        // Verify API key
        /*if (request.ApiKey != _photoValidationOptions.WebhookApiKey)
        {
            return Unauthorized();
        }*/

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