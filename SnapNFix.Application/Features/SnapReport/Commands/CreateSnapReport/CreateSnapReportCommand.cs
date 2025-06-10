using MediatR;
using Microsoft.AspNetCore.Http;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;

public class CreateSnapReportCommand : IRequest<GenericResponseModel<ReportDetailsDto>>
{
    public string Comment { get; set; } = string.Empty;
    public IFormFile Image { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Severity Severity { get; set; } = Severity.Medium;
}