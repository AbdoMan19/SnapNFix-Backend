using SnapNFix.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing.Dto_s;

public class AiValidationWebhookDto
{
    [Required]
    public string TaskId { get; set; } = string.Empty;
    
    [Required]
    public string Status { get; set; } = string.Empty; 
    
    public string? Category { get; set; } 
    
    public double Threshold { get; set; } 
    
    public ImageStatus ImageStatus => MapStatus();
    
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