using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.CityChannel.DTOs;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.CityChannel.Queries.GetCityChannelIssues;

public class GetCityChannelIssuesQueryHandler : IRequestHandler<GetCityChannelIssuesQuery, GenericResponseModel<PagedList<CityIssueDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCityChannelIssuesQueryHandler> _logger;

    public GetCityChannelIssuesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetCityChannelIssuesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<GenericResponseModel<PagedList<CityIssueDto>>> Handle(
        GetCityChannelIssuesQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify city exists
            var cityChannel = await _unitOfWork.Repository<Domain.Entities.CityChannel>()
                .FindBy(cc => cc.Id == request.CityId).FirstOrDefaultAsync(cancellationToken);
                
            if (cityChannel == null)
            {
                return GenericResponseModel<PagedList<CityIssueDto>>.Failure(
                    "City not found",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.CityId), "City with the provided ID does not exist.")
                    });
            }
            
            // Get issues for this city
            var query = _unitOfWork.Repository<Domain.Entities.Issue>()
                .GetQuerableData()
                .Where(i => i.City == cityChannel.Name && i.State == cityChannel.State);
            
            // Apply status filter if provided
            if (request.StatusFilter.HasValue)
            {
                query = query.Where(i => i.Status == request.StatusFilter.Value);
            }
            
            // Order by created date, newest first
            query = query.OrderByDescending(i => i.CreatedAt);
            
            // Create paged list
            var issues = await PagedList<Domain.Entities.Issue>.CreateAsync(
                query, 
                request.PageNumber, 
                request.PageSize,
                cancellationToken);
            
            // Map to DTOs
            var issueDtos = _mapper.Map<List<CityIssueDto>>(issues.Items);
            
            // Create paged list of DTOs
            var result = new PagedList<CityIssueDto>(
                issueDtos,
                issues.TotalCount,
                issues.PageNumber,
                issues.PageSize);
            
            return GenericResponseModel<PagedList<CityIssueDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving issues for city {CityId}", request.CityId);
            return GenericResponseModel<PagedList<CityIssueDto>>.Failure(Shared.OperationFailed);
        }
    }
}