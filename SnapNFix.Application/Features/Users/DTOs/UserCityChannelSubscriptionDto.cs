namespace SnapNFix.Application.Features.Users.DTOs
{
    public class UserCityChannelSubscriptionDto
    {
       public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public int ActiveIssuesCount { get; set; } 
    }
}