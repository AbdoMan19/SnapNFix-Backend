using MediatR;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Features.FastReport.Create;

namespace SnapNFix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FastReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public FastReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateFastReportCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
}