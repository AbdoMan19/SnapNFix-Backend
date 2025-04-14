using MediatR;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Users.Commands.RegisterUser;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitizensController : ControllerBase
{
    private readonly IMediator _mediator;

    public CitizensController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<GenericResponseModel<Guid>>> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);

        return result;
    }
}