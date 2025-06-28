namespace SnapNFix.Domain.Entities
{
    public class AdminTarget
    {
        public Guid Id { get; set; }
        public double TargetResolutionRate { get; set; } = 95.0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; } 
        public User CreatedByUser { get; set; }
        public bool IsActive { get; set; } = true;
    }
}