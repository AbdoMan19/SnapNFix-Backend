using SnapNFix.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing.Dto_s;

public class AiValidationWebhookDto
{
    [Required]
    [JsonPropertyName("task_id")]  
    public string TaskId { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("status")]  
    public string Status { get; set; } = string.Empty; 
    
    [JsonPropertyName("category")] 
    public string? Category { get; set; } 
    
    [JsonPropertyName("threshold")] 
    public double Threshold { get; set; } 
    
    [JsonIgnore]
    public ImageStatus ImageStatus => MapStatus();
    
    [JsonIgnore]
    public ReportCategory ReportCategory => MapCategory();
    
    private ImageStatus MapStatus()
    {
        return Status?.ToLower() switch
        {
            "completed" when Threshold >= 0.3 => ImageStatus.Approved,
            "completed" when Threshold < 0.3 => ImageStatus.Declined,
            "failed" => ImageStatus.Declined,
            _ => ImageStatus.Pending
        };
    }
    
    private ReportCategory MapCategory()
    {
        if (string.IsNullOrEmpty(Category))
            return ReportCategory.NotSpecified;
            
        return Category.ToLower() switch
        {
            "garbage" => ReportCategory.Garbage,
            "pothole" => ReportCategory.Pothole,
            "defective_manhole" or "defectivemanhole" => ReportCategory.DefectiveManhole,
            "non_defective_manhole" or "nondefectivemanhole" => ReportCategory.NonDefectiveManhole,
            _ => ReportCategory.NotSpecified
        };
    }
}