namespace SnapNFix.Domain.Entities;

public class Issue
{
    public Guid Id { get; set; }
    
    public Guid MainReportId { get; set; }
    public SnapReport MainReport { get; set; }

    //public virtual ICollection<SnapReport> AssociatedSnapReports { get; set; } = [];
    public virtual ICollection<FastReport> AssociatedFastReports { get; set; } = [];
}