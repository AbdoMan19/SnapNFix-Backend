using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Infrastructure.Context;
using System.Text;
using SnapNFix.Application.Interfaces;

namespace SnapNFix.Infrastructure.Services.DataSeeding;

public class DataSeedingService : IDataSeedingService
{
    private readonly SnapNFixContext _context;
    private readonly ILogger<DataSeedingService> _logger;
    private readonly Random _random;
    private readonly GeometryFactory _geometryFactory;

    // Egypt boundaries for realistic location generation
    private const double EgyptMinLat = 22.0;
    private const double EgyptMaxLat = 31.7;
    private const double EgyptMinLng = 25.0;
    private const double EgyptMaxLng = 35.0;

    // Common Egyptian cities for realistic addresses
    private readonly string[] _egyptianCities = {
        "Cairo", "Alexandria", "Giza", "Shubra El Kheima", "Port Said",
        "Suez", "Luxor", "Aswan", "Asyut", "Ismailia", "Fayyum",
        "Zagazig", "Damietta", "Aswan", "Minya", "Damanhur", "Beni Suef",
        "Hurghada", "Qena", "Sohag", "Shibin El Kom", "Tanta", "Kafr El Sheikh",
        "Mallawi", "Dikirnis", "Bilbais", "Arish", "Idfu", "Mit Ghamr"
    };

    private readonly string[] _roadNames = {
        "Tahrir Square", "Nile Corniche", "Salah Salem", "Ahmed Helmi",
        "Ramses Street", "26th July Street", "Qasr Al Nil", "Mohamed Ali Street",
        "Al Azhar Street", "Port Said Street", "Champollion Street",
        "Kasr El Ainy Street", "Abbas El Akkad", "Makram Ebeid",
        "El Nasr Road", "Ring Road", "Desert Road", "Agricultural Road"
    };

    private readonly string[] _commentTemplates = {
        "There's a large pothole here that needs immediate attention",
        "Garbage has been accumulating for days",
        "This manhole cover is damaged and dangerous",
        "Road surface is severely cracked",
        "Waste bins are overflowing",
        "Street lighting is not working",
        "This area needs urgent repair",
        "Safety hazard for pedestrians and vehicles",
        "Poor road conditions affecting traffic",
        "Environmental health concern in this location"
    };

    public DataSeedingService(SnapNFixContext context, ILogger<DataSeedingService> logger)
    {
        _context = context;
        _logger = logger;
        _random = new Random();
        _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
    }

    public async Task SeedLargeDatasetAsync(int numberOfUsers = 1000, int numberOfReports = 10000)
    {
        _logger.LogInformation("Starting to seed {UserCount} users and {ReportCount} reports", numberOfUsers, numberOfReports);

        try
        {
            // Step 1: Create users in batches using raw SQL for performance
            await CreateUsersInBulk(numberOfUsers);

            // Step 2: Create reports and issues in batches
            await CreateReportsAndIssuesInBulk(numberOfReports);

            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data seeding");
            throw;
        }
    }

    private async Task CreateUsersInBulk(int userCount)
    {
        _logger.LogInformation("Creating {UserCount} users in bulk", userCount);

        var batchSize = 1000;
        var batches = (int)Math.Ceiling((double)userCount / batchSize);

        for (int batch = 0; batch < batches; batch++)
        {
            var currentBatchSize = Math.Min(batchSize, userCount - (batch * batchSize));
            var usersSql = GenerateUsersInsertSql(currentBatchSize, batch * batchSize);
            
            await _context.Database.ExecuteSqlRawAsync(usersSql);
            _logger.LogInformation("Created batch {Batch}/{TotalBatches} ({BatchSize} users)", 
                batch + 1, batches, currentBatchSize);
        }
    }

    private async Task CreateReportsAndIssuesInBulk(int reportCount)
    {
        _logger.LogInformation("Creating {ReportCount} reports and associated issues", reportCount);

        // First, get user IDs to assign reports to
        var userIds = await _context.Set<User>()
            .Select(u => u.Id)
            .Take(Math.Min(1000, reportCount)) // Use subset of users
            .ToListAsync();

        if (!userIds.Any())
        {
            throw new InvalidOperationException("No users found. Please seed users first.");
        }

        var batchSize = 500;
        var batches = (int)Math.Ceiling((double)reportCount / batchSize);

        for (int batch = 0; batch < batches; batch++)
        {
            var currentBatchSize = Math.Min(batchSize, reportCount - (batch * batchSize));
            
            // Create issues first
            var issuesCreated = await CreateIssuesInBulk(currentBatchSize);
            
            // Create reports and link them to issues (avoiding the unique constraint issue)
            var reportsSql = GenerateReportsWithIssuesInsertSql(currentBatchSize, userIds, issuesCreated);
            await _context.Database.ExecuteSqlRawAsync(reportsSql);

            _logger.LogInformation("Created batch {Batch}/{TotalBatches} ({BatchSize} reports)", 
                batch + 1, batches, currentBatchSize);
        }
    }

    private async Task<List<Guid>> CreateIssuesInBulk(int count)
    {
        var issueIds = new List<Guid>();
        var issuesSql = GenerateIssuesInsertSqlWithIds(count, issueIds);
        await _context.Database.ExecuteSqlRawAsync(issuesSql);
        return issueIds;
    }

    private string GenerateUsersInsertSql(int count, int startIndex)
    {
        var sql = new StringBuilder();
        sql.AppendLine("INSERT INTO \"Users\" (\"Id\", \"FirstName\", \"LastName\", \"PhoneNumber\", \"UserName\", \"NormalizedUserName\", \"Email\", \"NormalizedEmail\", \"EmailConfirmed\", \"PasswordHash\", \"SecurityStamp\", \"ConcurrencyStamp\", \"PhoneNumberConfirmed\", \"TwoFactorEnabled\", \"LockoutEnabled\", \"AccessFailedCount\", \"CreatedAt\", \"UpdatedAt\", \"IsDeleted\", \"ImagePath\", \"Username\", \"Gender\")");
        sql.AppendLine("VALUES");

        var values = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var userId = Guid.NewGuid();
            var firstName = GenerateRandomFirstName();
            var lastName = GenerateRandomLastName();
            var phoneNumber = GenerateEgyptianPhoneNumber();
            var email = $"user{startIndex + i}@snapnfix.com";
            var userName = phoneNumber;
            var passwordHash = "$2a$11$8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8Z8"; // Dummy hash
            var securityStamp = Guid.NewGuid().ToString();
            var concurrencyStamp = Guid.NewGuid().ToString();
            var createdAt = DateTime.UtcNow.AddDays(-_random.Next(0, 365));
            var gender = ((Gender)_random.Next(0, 3)).ToString();

            values.Add($"('{userId}', '{firstName}', '{lastName}', '{phoneNumber}', '{userName}', '{userName.ToUpper()}', '{email}', '{email.ToUpper()}', true, '{passwordHash}', '{securityStamp}', '{concurrencyStamp}', true, false, false, 0, '{createdAt:yyyy-MM-dd}', '{createdAt:yyyy-MM-dd}', false, '', '{Guid.NewGuid()}', '{gender}')");
        }

        sql.AppendLine(string.Join(",\n", values));
        return sql.ToString();
    }

    private string GenerateReportsInsertSql(int count, List<Guid> userIds)
    {
        var sql = new StringBuilder();
        sql.AppendLine("INSERT INTO \"SnapReports\" (\"Id\", \"UserId\", \"Comment\", \"ImagePath\", \"Location\", \"Road\", \"City\", \"State\", \"Country\", \"ImageStatus\", \"ReportCategory\", \"Severity\", \"CreatedAt\", \"IsDeleted\")");
        sql.AppendLine("VALUES");

        var values = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var reportId = Guid.NewGuid();
            var userId = userIds[_random.Next(userIds.Count)];
            var comment = EscapeSqlString(_commentTemplates[_random.Next(_commentTemplates.Length)]);
            var imagePath = EscapeSqlString($"https://snapnfixstorage.blob.core.windows.net/snapreports/{Guid.NewGuid()}.jpg");
            var location = GenerateRandomEgyptLocation();
            var road = EscapeSqlString(_roadNames[_random.Next(_roadNames.Length)]);
            var city = EscapeSqlString(_egyptianCities[_random.Next(_egyptianCities.Length)]);
            var state = EscapeSqlString(GenerateRandomState());
            var country = EscapeSqlString("Egypt");
            var status = GenerateRandomImageStatus();
            var category = GenerateRandomReportCategory();
            var severity = GenerateRandomSeverity();
            var createdAt = DateTime.UtcNow.AddDays(-_random.Next(0, 90));

            values.Add($"('{reportId}', '{userId}', '{comment}', '{imagePath}', ST_SetSRID(ST_MakePoint({location.longitude}, {location.latitude}), 4326), '{road}', '{city}', '{state}', '{country}', '{status}', '{category}', '{severity}', '{createdAt:yyyy-MM-dd HH:mm:ss}', false)");
        }

        sql.AppendLine(string.Join(",\n", values));
        return sql.ToString();
    }

    private string GenerateReportsWithIssuesInsertSql(int count, List<Guid> userIds, List<Guid> issueIds)
    {
        var sql = new StringBuilder();
        sql.AppendLine("INSERT INTO \"SnapReports\" (\"Id\", \"UserId\", \"IssueId\", \"Comment\", \"ImagePath\", \"Location\", \"Road\", \"City\", \"State\", \"Country\", \"ImageStatus\", \"ReportCategory\", \"Severity\", \"CreatedAt\", \"IsDeleted\")");
        sql.AppendLine("VALUES");

        var values = new List<string>();
        var usedUserIssueCombo = new HashSet<string>(); // Track used combinations

        for (int i = 0; i < count; i++)
        {
            var reportId = Guid.NewGuid();
            var status = GenerateRandomImageStatus();
            
            Guid? issueId = null;
            Guid userId;
            string userIssueKey;
            
            // Try to find a valid user-issue combination that doesn't violate the unique constraint
            int attempts = 0;
            do
            {
                userId = userIds[_random.Next(userIds.Count)];
                
                // Only assign issue for approved reports
                if (status == "Approved" && issueIds.Any())
                {
                    issueId = issueIds[_random.Next(issueIds.Count)];
                    userIssueKey = $"{userId}_{issueId}";
                }
                else
                {
                    userIssueKey = $"{userId}_null";
                }
                
                attempts++;
                if (attempts > 10) // Fallback: create without issue
                {
                    issueId = null;
                    userIssueKey = $"{userId}_null_{_random.Next(1000000)}"; // Make it unique
                    break;
                }
            } 
            while (usedUserIssueCombo.Contains(userIssueKey));
            
            usedUserIssueCombo.Add(userIssueKey);

            var comment = EscapeSqlString(_commentTemplates[_random.Next(_commentTemplates.Length)]);
            var imagePath = EscapeSqlString($"https://snapnfixstorage.blob.core.windows.net/snapreports/{Guid.NewGuid()}.jpg");
            var location = GenerateRandomEgyptLocation();
            var road = EscapeSqlString(_roadNames[_random.Next(_roadNames.Length)]);
            var city = EscapeSqlString(_egyptianCities[_random.Next(_egyptianCities.Length)]);
            var state = EscapeSqlString(GenerateRandomState());
            var country = EscapeSqlString("Egypt");
            var category = GenerateRandomReportCategory();
            var severity = GenerateRandomSeverity();
            var createdAt = DateTime.UtcNow.AddDays(-_random.Next(0, 90));

            var issueIdValue = issueId.HasValue ? $"'{issueId.Value}'" : "NULL";

            values.Add($"('{reportId}', '{userId}', {issueIdValue}, '{comment}', '{imagePath}', ST_SetSRID(ST_MakePoint({location.longitude}, {location.latitude}), 4326), '{road}', '{city}', '{state}', '{country}', '{status}', '{category}', '{severity}', '{createdAt:yyyy-MM-dd HH:mm:ss}', false)");
        }

        sql.AppendLine(string.Join(",\n", values));
        return sql.ToString();
    }

    private string GenerateIssuesInsertSqlWithIds(int count, List<Guid> issueIds)
    {
        var sql = new StringBuilder();
        sql.AppendLine("INSERT INTO \"Issues\" (\"Id\", \"Category\", \"Location\", \"Road\", \"City\", \"State\", \"Country\", \"Status\", \"Severity\", \"ImagePath\", \"CreatedAt\")");
        sql.AppendLine("VALUES");

        var values = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var issueId = Guid.NewGuid();
            issueIds.Add(issueId); 
            
            var category = GenerateRandomReportCategory();
            var location = GenerateRandomEgyptLocation();
            var road = _roadNames[_random.Next(_roadNames.Length)];
            var city = _egyptianCities[_random.Next(_egyptianCities.Length)];
            var state = GenerateRandomState();
            var country = "Egypt";
            var status = GenerateRandomIssueStatus();
            var severity = GenerateRandomSeverity();
            var imagePath = $"https://snapnfixstorage.blob.core.windows.net/issues/{Guid.NewGuid()}.jpg";
            var createdAt = DateTime.UtcNow.AddDays(-_random.Next(0, 60));

            values.Add($"('{issueId}', '{category}', ST_SetSRID(ST_MakePoint({location.longitude}, {location.latitude}), 4326), '{road}', '{city}', '{state}', '{country}', '{status}', '{severity}', '{imagePath}', '{createdAt:yyyy-MM-dd HH:mm:ss}')");
        }

        sql.AppendLine(string.Join(",\n", values));
        return sql.ToString();
    }

    private string EscapeSqlString(string value)
    {
        return value.Replace("'", "''");
    }


    private (double latitude, double longitude) GenerateRandomEgyptLocation()
    {
        var latitude = EgyptMinLat + (_random.NextDouble() * (EgyptMaxLat - EgyptMinLat));
        var longitude = EgyptMinLng + (_random.NextDouble() * (EgyptMaxLng - EgyptMinLng));
        return (Math.Round(latitude, 6), Math.Round(longitude, 6));
    }

    private string GenerateRandomFirstName()
    {
        var names = new[] { "Ahmed", "Mohamed", "Mahmoud", "Omar", "Ali", "Hassan", "Ibrahim", "Youssef", "Amr", "Khaled", "Fatma", "Aisha", "Nour", "Yasmin", "Aya", "Salma", "Nada", "Rania", "Dina", "Heba" };
        return names[_random.Next(names.Length)];
    }

    private string GenerateRandomLastName()
    {
        var names = new[] { "Hassan", "Mohamed", "Ahmed", "Ali", "Mahmoud", "Ibrahim", "Omar", "Youssef", "Abdel Rahman", "El Sayed", "Farouk", "Mansour", "Rashad", "Zaki", "Nasser", "Said", "Kamal", "Fathy", "Salah", "Helmy" };
        return names[_random.Next(names.Length)];
    }

    private string GenerateEgyptianPhoneNumber()
    {
        var prefixes = new[] { "010", "011", "012", "015" };
        var prefix = prefixes[_random.Next(prefixes.Length)];
        var number = _random.Next(10000000, 99999999);
        return $"{prefix}{number}";
    }

    private string GenerateRandomState()
    {
        var states = new[] { "Cairo", "Alexandria", "Giza", "Qalyubia", "Port Said", "Suez", "Luxor", "Aswan", "Asyut", "Beheira", "Beni Suef", "Dakahlia", "Damietta", "Fayyum", "Gharbia", "Ismailia", "Kafr El Sheikh", "Matrouh", "Minya", "Monufia", "New Valley", "North Sinai", "Qena", "Red Sea", "Sharqia", "Sohag", "South Sinai" };
        return states[_random.Next(states.Length)];
    }

    private string GenerateRandomImageStatus()
    {
        var statuses = new[] { "Pending", "Approved", "Declined" };
        var weights = new[] { 0.2, 0.7, 0.1 }; // 70% approved, 20% pending, 10% declined
        var rand = _random.NextDouble();
        
        if (rand < weights[0]) return statuses[0];
        if (rand < weights[0] + weights[1]) return statuses[1];
        return statuses[2];
    }

    private string GenerateRandomReportCategory()
    {
        var categories = Enum.GetNames<ReportCategory>();
        return categories[_random.Next(categories.Length)];
    }

    private string GenerateRandomIssueStatus()
    {
        var statuses = Enum.GetNames<IssueStatus>();
        return statuses[_random.Next(statuses.Length)];
    }

    private string GenerateRandomSeverity()
    {
        var severities = Enum.GetNames<Severity>();
        return severities[_random.Next(severities.Length)];
    }

    public async Task ClearAllDataAsync()
    {
        _logger.LogWarning("Clearing all data from database");

        // Clear in correct order due to foreign key constraints
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"FastReport\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"SnapReports\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Issues\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"RefreshToken\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"UserDevice\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"UserRoles\"");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Users\" WHERE \"Email\" LIKE '%@snapnfix.com'"); // Only delete seeded users
        
        _logger.LogInformation("All seeded data cleared successfully");
    }
}

// Extension method to easily use the seeding service
public static class DataSeedingExtensions
{
    public static async Task SeedDummyDataAsync(this IServiceProvider serviceProvider, int users = 1000, int reports = 10000)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SnapNFixContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataSeedingService>>();
        
        var seedingService = new DataSeedingService(context, logger);
        await seedingService.SeedLargeDatasetAsync(users, reports);
    }
}
