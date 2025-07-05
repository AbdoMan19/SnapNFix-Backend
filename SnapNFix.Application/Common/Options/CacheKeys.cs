namespace SnapNFix.Application.Common.Options;
public static class CacheKeys
{

    public static string IssueDetails(Guid issueId) => $"issue:details:{issueId}";
    public static string NearbyIssues(double neLat, double neLng, double swLat, double swLng) =>
        $"issues:nearby:{neLat:F4}:{neLng:F4}:{swLat:F4}:{swLng:F4}";

    public static string ReportDetails(Guid reportId) => $"report:details:{reportId}";

    public const string DashboardSummary = "dashboard_summary";
    public const string MetricsOverview = "metrics_overview";
    public const string MonthlyTarget = "monthly_target";
    public const string FullStatistics = "full_statistics";
    public const string CategoryDistribution = "category_distribution";
    public static string GeographicDistribution => $"geographic_distribution";
    public static string IncidentTrends(StatisticsInterval interval) => $"incident_trends_{interval}";

}