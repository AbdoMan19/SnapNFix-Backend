using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SnapNFix.Api.Configuration;

public static class RateLimitingConfiguration
{
    public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
    {
        // Add IP Rate Limiting Service
        // services.AddSingleton<IpRateLimiter>();
        
        // Add General Rate Limiting
        services.AddRateLimiter(options =>
        {
            // Global API Rate Limiting
            options.AddTokenBucketLimiter("GlobalApi", opt =>
            {
                opt.TokenLimit = 100;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
                opt.ReplenishmentPeriod = TimeSpan.FromSeconds(1);
                opt.TokensPerPeriod = 10;
                opt.AutoReplenishment = true;
            });

            // Auth endpoints
            options.AddFixedWindowLimiter("AuthApi", opt =>
            {
                opt.PermitLimit = 5;
                opt.Window = TimeSpan.FromMinutes(5);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });

            // User specific endpoints
            options.AddSlidingWindowLimiter("UserBasedApi", opt =>
            {
                opt.PermitLimit = 30;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.SegmentsPerWindow = 6;
                opt.QueueLimit = 5;
            });

            options.OnRejected = async (context, token) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = 
                        ((int)retryAfter.TotalSeconds).ToString();
                }
                
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Error = "Too many requests. Please try again later.",
                    RetryAfter = retryAfter
                }, token);
            };
        });
        return services;
    }
}