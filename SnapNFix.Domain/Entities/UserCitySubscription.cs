using System;

namespace SnapNFix.Domain.Entities
{
    public class UserCitySubscription
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Guid CityChannelId { get; set; }
        public virtual CityChannel CityChannel { get; set; }
        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}