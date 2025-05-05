/*using System.Net;
using SnapNFix.Api.RateLimiting;
using SnapNFix.Api.RateLimiting.Interfaces;

namespace SnapNFix.Api.Middleware;

public class IpRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Dictionary<string, RateLimitRule> _rules;
    private readonly ILogger<IpRateLimitingMiddleware> _logger;
    private readonly IRateLimiter _rateLimiter;

    public IpRateLimitingMiddleware(
        RequestDelegate next,
        ILogger<IpRateLimitingMiddleware> logger,
        IRateLimiter rateLimiter)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _rules = IpRateLimitingOptions.GetRules();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress;
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (ipAddress == null || path == null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        var rule = _rules.GetValueOrDefault(path) ?? _rules["*"];
        
        if (await _rateLimiter.IsIpBlockedAsync(ipAddress, path, rule, context.RequestAborted))
        {
            _logger.LogWarning("Rate limit exceeded for IP {IpAddress} on path {Path}", ipAddress, path);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new 
            { 
                error = "Too many requests. Please try again later.",
                retryAfter = $"{rule.WindowInMinutes} minutes"
            }, context.RequestAborted);
            return;
        }

        await _next(context);
    }
}*/