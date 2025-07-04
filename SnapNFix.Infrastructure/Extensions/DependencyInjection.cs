using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
using SnapNFix.Infrastructure.Context;
using SnapNFix.Infrastructure.Options;
using SnapNFix.Infrastructure.Services.BackgroundTasksService;

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
                    npgsqlOptions.CommandTimeout(30);
                });
        });
     
        services.AddMemoryCache();
        
        services.AddHttpClient();
        
        services.Configure<ImageProcessingSettings>(
            configuration.GetSection("ImageProcessing"));
        
        services.Configure<PhotoValidationOptions>(
            configuration.GetSection("AI").GetSection("PhotoValidation"));

        services.AddHostedService<BackgroundTaskExecutor>();
      
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
