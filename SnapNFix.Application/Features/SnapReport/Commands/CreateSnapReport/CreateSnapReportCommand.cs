using MediatR;
using NetTopologySuite.Geometries;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;
public class CreateSnapReportCommand : IRequest<GenericResponseModel<ReportDetailsDto>>
{
    public string? Comment { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public ReportCategory Category { get; set; }
    public Guid UserId { get; set; }

}

