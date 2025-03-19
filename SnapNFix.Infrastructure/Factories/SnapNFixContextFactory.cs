/*using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SnapNFix.Infrastructure.Context;
using System.IO;

namespace SnapNFix.Infrastructure.Factories
{
    public class SnapNFixContextFactory : IDesignTimeDbContextFactory<SnapNFixContext>
    {
        public SnapNFixContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Will point to SnapNFix folder when running EF commands
                .AddJsonFile("appsettings.Development.json", optional: true) // Adjust if needed
                .AddJsonFile("appsettings.json", optional: true) // fallback
                .Build();

            // Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<SnapNFixContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseNetTopologySuite(); // Required for PostGIS
            });

            return new SnapNFixContext(optionsBuilder.Options);
        }
    }
}*/