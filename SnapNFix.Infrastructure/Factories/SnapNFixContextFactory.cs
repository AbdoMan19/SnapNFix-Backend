using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SnapNFix.Infrastructure.Context;

namespace SnapNFix.Infrastructure.Factories;

public class SnapNFixContextFactory : IDesignTimeDbContextFactory<SnapNFixContext>
{
    public SnapNFixContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../SnapNFix.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true);

        var configuration = configurationBuilder.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<SnapNFixContext>()
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseNetTopologySuite();
            });

        return new SnapNFixContext(optionsBuilder.Options);
    }
}