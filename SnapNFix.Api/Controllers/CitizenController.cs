using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Users.Commands.RegisterUser;
using SnapNFix.Application.Features.Users.Commands.UpdateUser;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitizenController : ControllerBase
{
    private readonly IMediator _mediator;

    public CitizenController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("profile")]
    [Authorize("Citizen")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
}