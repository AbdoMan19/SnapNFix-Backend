using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Users.Commands.RegisterUser;
using SnapNFix.Application.Features.Users.Commands.UpdateUser;
using SnapNFix.Application.Resources;

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
        if (command == null)
        {
            var response = GenericResponseModel<bool>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(command), "Invalid request body or invalid enum value.")
                });
            return BadRequest(response);
        }
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
}