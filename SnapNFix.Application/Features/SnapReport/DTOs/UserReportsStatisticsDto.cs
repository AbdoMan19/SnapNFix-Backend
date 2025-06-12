using System.Collections.Generic;

namespace SnapNFix.Application.Features.SnapReport.DTOs;

public class UserReportsStatisticsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ApprovedReports { get; set; }

}