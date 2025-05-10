using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;

public class ImageValidationResultCommand : IRequest<GenericResponseModel<bool>>
{
    public string TaskId { get; set; }
    public ImageStatus ImageStatus { get; set; }
    public ReportCategory ReportCategory { get; set; }
    public double Threshold { get; set; }
}