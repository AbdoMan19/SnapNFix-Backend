using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing.Dto_s;

public class AiValidationWebhookDto
{
    public string TaskId { get; set; }
    public ImageStatus ImageStatus { get; set; }
    public ReportCategory Category { get; set; }
    public double Threshold { get; set; }
    public string ApiKey { get; set; }
}