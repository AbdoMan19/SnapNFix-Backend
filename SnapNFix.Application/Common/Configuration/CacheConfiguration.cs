namespace SnapNFix.Application.Common.Configuration;

public static class CacheConfiguration
{
    public static class Keys
    {
        // Issue-related cache keys
        public const string IssueDetails = "issue_details_{0}"; // {0} = issueId
        public const string IssuesList = "issues_{0}_{1}_{2}_{3}"; // {0} = status, {1} = category, {2} = page, {3} = size
        public const string NearbyIssues = "nearby_issues_{0:F4}_{1:F4}_{2:F4}_{3:F4}_{4}"; // {0-3} = coordinates, {4} = maxResults
        
        // User reports cache keys
        public const string UserReports = "user_reports_{0}_{1}_{2}_{3}_{4}"; // {0} = userId, {1} = status, {2} = category, {3} = page, {4} = size
        public const string UserReportsStats = "user_reports_stats_{0}"; // {0} = userId
        
        // Issue reports cache keys
        public const string IssueSnapReports = "issue_snap_reports_{0}_{1}"; // {0} = issueId, {1} = page
        public const string IssueFastReports = "issue_fast_reports_{0}_{1}"; // {0} = issueId, {1} = page
        
        // Statistics cache keys
        public const string DashboardSummary = "dashboard_summary";
        public const string MetricsOverview = "metrics_overview";
        public const string CategoryDistribution = "category_distribution";
        public const string MonthlyTarget = "monthly_target";
        public const string IncidentTrends = "incident_trends_{0}"; // {0} = interval
        public const string GeographicDistribution = "geographic_distribution_{0}"; // {0} = limit
        
        // Pattern keys for invalidation
        public const string PatternIssues = "issues_*";
        public const string PatternNearbyIssues = "nearby_issues_*";
        public const string PatternUserReports = "user_reports_{0}_*"; // {0} = userId
        public const string PatternIssueSnapReports = "issue_snap_reports_{0}_*"; // {0} = issueId
        public const string PatternIssueFastReports = "issue_fast_reports_{0}_*"; // {0} = issueId
        public const string PatternAllIssues = "issue_*";
        public const string PatternAllReports = "*reports*";
    }
    
    public static class TTL
    {
        // Issue-related TTL
        public static readonly TimeSpan IssueDetails = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan IssuesList = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan NearbyIssues = TimeSpan.FromMinutes(10);
        
        // User reports TTL
        public static readonly TimeSpan UserReports = TimeSpan.FromMinutes(20);
        public static readonly TimeSpan UserReportsStats = TimeSpan.FromMinutes(10);
        
        // Issue reports TTL
        public static readonly TimeSpan IssueSnapReports = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan IssueFastReports = TimeSpan.FromMinutes(15);
        
        // Statistics TTL
        public static readonly TimeSpan DashboardSummary = TimeSpan.FromMinutes(2);
        public static readonly TimeSpan MetricsOverview = TimeSpan.FromMinutes(2);
        public static readonly TimeSpan CategoryDistribution = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan MonthlyTarget = TimeSpan.FromMinutes(2);
        public static readonly TimeSpan IncidentTrends = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan GeographicDistribution = TimeSpan.FromMinutes(60);
        
        // Sliding expiration for frequently accessed items
        public static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(5);
    }
    
    public static class Patterns
    {
        public static string GetUserReportsPattern(Guid userId) => string.Format(Keys.PatternUserReports, userId);
        public static string GetIssueSnapReportsPattern(Guid issueId) => string.Format(Keys.PatternIssueSnapReports, issueId);
        public static string GetIssueFastReportsPattern(Guid issueId) => string.Format(Keys.PatternIssueFastReports, issueId);
    }
} 