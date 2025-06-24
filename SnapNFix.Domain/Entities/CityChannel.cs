using System;

namespace SnapNFix.Domain.Entities
{
    public class CityChannel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "Egypt";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property
        public virtual ICollection<UserCitySubscription> Subscriptions { get; set; } = new List<UserCitySubscription>();
    }
}