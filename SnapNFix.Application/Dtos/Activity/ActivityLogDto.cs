using System;

namespace Application.DTOs
{
    public class ActivityLogDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; }
        public Guid IssueId { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public object? AdditionalData { get; set; }

        public Responsible? Responsible { get; set; }
    }
    

    public class Responsible
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}