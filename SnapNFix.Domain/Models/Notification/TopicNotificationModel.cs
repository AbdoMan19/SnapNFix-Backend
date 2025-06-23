namespace SnapNFix.Domain.Models.Notification;

public class TopicNotificationModel
{
    public string Topic { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}