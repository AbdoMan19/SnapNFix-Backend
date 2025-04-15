using System.Reflection;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SnapNFix.Api.Handlers;
using SnapNFix.Domain.Entities;
using SnapNFix.Infrastructure.Context;

namespace SnapNFix.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(10);
            })
            .AddEntityFrameworkStores<SnapNFixContext>()
            .AddDefaultTokenProviders();

        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });
        
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("FixedWindowPolicy", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5; // Allow 5 requests
                limiterOptions.Window = TimeSpan.FromSeconds(10); // Per 10 seconds
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 2; // Allow 2 requests to queue
            });
        });

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddHealthChecks()
            .AddDbContextCheck<SnapNFixContext>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddAuthorization();

        services.AddEndpointsApiExplorer();


        services.AddProblemDetails();

        return services;
    }
}
