namespace SnapNFix.Infrastructure.Options;

public class RedisOptions
{
    public const string SectionName = "Redis";
    
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = "SnapNFix";
    public int ConnectRetry { get; set; } = 3;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
    public bool AbortConnect { get; set; } = false;
    public int DefaultDatabase { get; set; } = 0;
    public bool EnableLogging { get; set; } = true;
} 