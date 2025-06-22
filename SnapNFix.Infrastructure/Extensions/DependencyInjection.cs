using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scrutor;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
using SnapNFix.Domain.Entities;
using SnapNFix.Infrastructure.Context;
using SnapNFix.Infrastructure.Options;
using SnapNFix.Infrastructure.Services;
using StackExchange.Redis;

namespace SnapNFix.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.RegisterServicesWithLifetime(Assembly.GetExecutingAssembly());
        services.AddDbContext<SnapNFixContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite();
                    //npgsqlOptions.EnableRetryOnFailure(3);
                    npgsqlOptions.CommandTimeout(30);
                });
        });
        /*services.AddHealthChecks()
            .AddDbContextCheck<SnapNFixContext>();*/
        
        
        // Add additional infrastructure services
        services.AddMemoryCache();
        /*services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });*/
        
        services.AddHttpClient();
        
        services.Configure<ImageProcessingSettings>(
            configuration.GetSection("ImageProcessing"));
        
        services.Configure<PhotoValidationOptions>(
            configuration.GetSection("AI").GetSection("PhotoValidation"));
        
        services.Configure<RedisOptions>(
            configuration.GetSection("Redis"));

        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        
        /*services.AddStackExchangeRedisCache(options =>
        {
             options.Configuration = configuration.GetConnectionString("Redis");
             options.InstanceName = "SnapNFix:";
        });*/

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
