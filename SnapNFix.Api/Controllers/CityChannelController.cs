using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.CityChannel.Queries.GetCityChannelIssues;
using SnapNFix.Application.Features.CityChannel.Queries.GetCityChannelMetrics;
using SnapNFix.Application.Features.Users.Commands.SubscribeToCityChannel;
using SnapNFix.Application.Features.Users.Commands.UnsubscribeFromCityChannel;
using SnapNFix.Application.Features.Users.DTOs;
using SnapNFix.Application.Features.Users.Queries.GetAvailableCitiesChannelQueries;
using SnapNFix.Application.Features.Users.Queries.GetUserSubscribedCitiesChannel;
using SnapNFix.Domain.Enums;

namespace SnapNFix.API.Controllers
{
    [ApiController]
    [Route("api/city-channels")]
    [Authorize(Roles = "Citizen")]
    public class CityChannelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CityChannelController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("available")]
        [ProducesResponseType(typeof(GenericResponseModel<PagedList<CityChannelSubscriptionDto>>), 200)]
        public async Task<IActionResult> GetAvailableCityChannels([FromQuery] GetAvailableCitiesChannelQuery query)
        {
            var result = await _mediator.Send(query);
            return result.ErrorList.Count == 0 ? Ok(result) : BadRequest(result);
        }


        [HttpGet("subscribed")]
        [ProducesResponseType(typeof(GenericResponseModel<PagedList<CityChannelSubscriptionDto>>), 200)]
        public async Task<IActionResult> GetSubscribedCityChannels([FromQuery] GetUserSubscribedCitiesChannelCommand query)
        {
            var result = await _mediator.Send(query);
            return result.ErrorList.Count == 0 ? Ok(result) : BadRequest(result);
        }


        [HttpPost("subscribe")]
        [ProducesResponseType(typeof(GenericResponseModel<bool>), 200)]
        public async Task<IActionResult> SubscribeToCity([FromBody] SubscribeToCityChannelCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }


        [HttpDelete("subscribe/{cityId}")]
        [ProducesResponseType(typeof(GenericResponseModel<bool>), 200)]
        public async Task<IActionResult> UnsubscribeFromCity([FromRoute]Guid cityId)
        {
            var command = new UnsubscribeFromCityChannelCommand { CityId = cityId };
            var result = await _mediator.Send(command);
            return result.ErrorList.Count == 0 ? Ok(result) : BadRequest(result);
        }
        
        [HttpGet("{cityId}/metrics")]
        public async Task<IActionResult> GetCityMetrics([FromRoute] Guid cityId)
        {
            var query = new GetCityChannelMetricsQuery { CityId = cityId };
            var result = await _mediator.Send(query);
        
            return result.ErrorList.Count == 0 ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{cityId}/issues")]
        public async Task<IActionResult> GetCityIssues(
            [FromRoute] Guid cityId,
            [FromQuery] IssueStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetCityChannelIssuesQuery 
            { 
                CityId = cityId,
                StatusFilter = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        
            var result = await _mediator.Send(query);
        
            return result.ErrorList.Count == 0 ? Ok(result) : BadRequest(result);
        }
    }
}