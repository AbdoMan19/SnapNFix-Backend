using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using SnapNFix.Domain.Entities;
using SnapNFix.Infrastructure.Context;
using SnapNFix.Infrastructure.Services;

namespace SnapNFix.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {


        services.Scan(selector => selector
            .FromAssemblies(
                Assembly.GetExecutingAssembly()
            )
            .AddClasses()
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddMemoryCache();
        
        
        return services;
    }

}