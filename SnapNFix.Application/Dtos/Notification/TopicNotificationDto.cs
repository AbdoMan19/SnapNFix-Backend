namespace Application.DTOs;

public class TopicNotificationDto
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string Topic { get; set; }
    public IDictionary<string , string>? Data { get; set; }
}