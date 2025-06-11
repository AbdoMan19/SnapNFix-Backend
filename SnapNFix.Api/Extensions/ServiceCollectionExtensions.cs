using System.Reflection;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SnapNFix.Api.Handlers;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces.ServiceLifetime;
using SnapNFix.Infrastructure.Context;

namespace SnapNFix.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.RegisterServicesWithLifetime(Assembly.GetExecutingAssembly());

        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(1);
        });

        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.User.RequireUniqueEmail = false; 
            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(10);
            options.User.un
        })
        .AddEntityFrameworkStores<SnapNFixContext>()
        .AddDefaultTokenProviders()
        .AddUserValidator<OptionalEmailValidator<User>>();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });



        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHttpContextAccessor();

        services.AddEndpointsApiExplorer();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            
            options.AddPolicy("Citizen", policy => policy.RequireRole("Citizen"));

            options.AddPolicy("RequirePhoneVerification", policy =>
                    policy.RequireClaim("purpose", TokenPurpose.PhoneVerification.ToString())
                        .RequireClaim("contact"));
            
            options.AddPolicy("RequireRegistration", policy =>
                policy.RequireClaim("purpose", TokenPurpose.Registration.ToString())
                    .RequireClaim("contact"));
            options.AddPolicy("RequirePasswordResetVerification", policy => 
                policy.RequireClaim("purpose", TokenPurpose.PasswordResetVerification.ToString())
                .RequireClaim("contact"));

            options.AddPolicy("RequireResetPassword", policy => policy.RequireClaim("purpose", TokenPurpose.PasswordReset.ToString())
                .RequireClaim("contact"));

        });

        services.AddProblemDetails();

        return services;
    }

    private static void RegisterServicesWithLifetime(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(classes => classes
                .AssignableTo<IScoped>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes
                .AssignableTo<ISingleton>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes
                .AssignableTo<ITransient>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }
}
