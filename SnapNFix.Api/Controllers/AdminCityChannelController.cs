using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.API.Models;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Admin.Commands.UpdateCityChannelStatus;
using SnapNFix.Application.Features.Admin.DTOs;
using SnapNFix.Application.Features.Admin.Queries.GetCitiesChannel;

namespace SnapNFix.API.Controllers
{
    [ApiController]
    [Route("api/admin/city-channels")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminCityChannelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminCityChannelController(IMediator mediator)
        {
            _mediator = mediator;
        }

     
        [HttpGet]
        [ProducesResponseType(typeof(GenericResponseModel<PagedList<CityChannelDto>>), 200)]
        public async Task<IActionResult> GetCitiesChannel([FromQuery] GetCitiesChannelQuery query)
        {
            var result = await _mediator.Send(query);
            return result.ErrorList.Count == 0 ? Ok(result) : BadRequest(result);
        }

      
        [HttpPut("{cityId}/status")]
        [ProducesResponseType(typeof(GenericResponseModel<bool>), 200)]
        public async Task<IActionResult> UpdateCityChannelStatus([FromRoute]Guid cityId, [FromBody] UpdateStatusRequest request)
        {
            var command = new UpdateCityChannelStatusCommand
            {
                CityId = cityId,
                IsActive = request.IsActive
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}