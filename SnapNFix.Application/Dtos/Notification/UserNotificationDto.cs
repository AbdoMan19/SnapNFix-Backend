using System;

namespace Application.DTOs
{
    public class UserNotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public Guid UserId { get; set; }
        public IDictionary<string , string>? Data { get; set; }
    }
}