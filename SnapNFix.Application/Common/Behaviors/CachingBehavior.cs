using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnapNFix.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply caching for ICacheableQuery requests
        if (request is ICacheableQuery cacheableQuery)
        {
            var cacheKey = cacheableQuery.CacheKey;
            
            // Try to get the item from cache
            try
            {
                var cachedResponse = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
                
                if (cachedResponse != null)
                {
                    _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                    return cachedResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from cache for key: {CacheKey}", cacheKey);
            }

            // Cache miss - execute the handler
            var response = await next();

            // If the response is successful, cache it
            if (IsSuccessfulResponse(response))
            {
                try
                {
                    await _cacheService.SetAsync(
                        cacheKey,
                        response,
                        cacheableQuery.CacheExpiration,
                        cancellationToken);
                    
                    _logger.LogInformation("Cached response for key: {CacheKey}", cacheKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error caching response for key: {CacheKey}", cacheKey);
                }
            }

            return response;
        }

        // Not a cacheable query, just execute the handler
        return await next();
    }

    private bool IsSuccessfulResponse(TResponse response)
    {
        // If the response is a BaseResponseModel, check if it's successful
        if (response is BaseResponseModel baseResponse)
        {
            return baseResponse.ErrorList.Count==0;
        }
        
        // For other types, assume success
        return true;
    }
}