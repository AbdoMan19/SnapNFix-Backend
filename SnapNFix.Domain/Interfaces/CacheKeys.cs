public static class CacheKeys
{
    // User related
    public static string UserProfile(Guid userId) => $"user:profile:{userId}";
    public static string UserReports(Guid userId, int page, string? status = null) => 
        $"user:reports:{userId}:page:{page}:status:{status ?? "all"}";
    public static string UserStatistics(Guid userId) => $"user:statistics:{userId}";
    
    // Issues related
    public static string IssueDetails(Guid issueId) => $"issue:details:{issueId}";
    public static string IssuesList(int page, string? status = null, string? category = null) => 
        $"issues:list:page:{page}:status:{status ?? "all"}:category:{category ?? "all"}";
    public static string NearbyIssues(double neLat, double neLng, double swLat, double swLng) => 
        $"issues:nearby:{neLat:F4}:{neLng:F4}:{swLat:F4}:{swLng:F4}";
    
    // Reports related
    public static string ReportDetails(Guid reportId) => $"report:details:{reportId}";
    public static string IssueReports(Guid issueId, int page) => $"issue:reports:{issueId}:page:{page}";
    public static string IssueFastReports(Guid issueId, int page) => $"issue:fast-reports:{issueId}:page:{page}";
    
    // Statistics 
    public const string DashboardSummary = "dashboard_summary";
    public const string MetricsOverview = "metrics_overview";
    public const string MonthlyTarget = "monthly_target";
    public const string FullStatistics = "full_statistics";
    
    // Patterns for bulk removal
    public const string UserPattern = "user:";
    public const string IssuePattern = "issue:";
    public const string ReportPattern = "report:";
    public const string StatisticsPattern = "_summary|_overview|_target|_statistics";
}