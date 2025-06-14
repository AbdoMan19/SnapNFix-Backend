public class StatisticsDto
{
    public MetricsOverviewDto Metrics { get; set; } = new();
    public List<CategoryDistributionDto> CategoryDistribution { get; set; } = new();
    public MonthlyTargetDto MonthlyTarget { get; set; } = new();
    public List<IncidentTrendDto> IncidentTrends { get; set; } = new();
}

public class MetricsOverviewDto
{
    public int TotalIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int PendingIncidents { get; set; }
    public double ResolutionRate { get; set; }
    public int NewThisMonth { get; set; }
    public double MonthlyGrowthPercentage { get; set; }
}

public class CategoryDistributionDto
{
    public string Category { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Resolved { get; set; }
    public int Pending { get; set; }
    public double Percentage { get; set; }
}

public class MonthlyTargetDto
{
    public double TargetResolutionRate { get; set; } = 95.0;
    public double CurrentResolutionRate { get; set; }
    public double Progress { get; set; }
    public int IncidentsToTarget { get; set; }
    public string Status { get; set; } = string.Empty; // "On Track", "Behind", "Ahead"
}

public class IncidentTrendDto
{
    public string Period { get; set; } = string.Empty; // "Jan", "Feb", "Q1", "2024"
    public int TotalIncidents { get; set; }
    public int ResolvedIncidents { get; set; }
    public int PendingIncidents { get; set; }
    public DateTime Date { get; set; }
}

public class GeographicDistributionDto
{
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int IncidentCount { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}