using Mapster;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Common.Mapping;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LoginWithPhoneOrEmailCommand, UserDevice>();
        
        TypeAdapterConfig<Domain.Entities.SnapReport, ReportDetailsDto>
            .NewConfig()
            .Map(dest => dest.Latitude, src => src.Location != null ? src.Location.Y : 0)
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0)
            .Map(dest => dest.Status, src => src.ImageStatus) 
            .Map(dest => dest.Category, src => src.ReportCategory)
            .Map(dest => dest.Severity, src => src.Severity); 

        TypeAdapterConfig<Domain.Entities.SnapReport, IssueDetailsDto>
            .NewConfig()
            .Map(dest => dest.Latitude, src => src.Location != null ? src.Location.Y : 0)
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0)
            .Map(dest => dest.Category, src => src.ReportCategory.ToString())
            .Map(dest => dest.Status, src => src.Issue != null ? src.Issue.Status.ToString() : "Unknown")
            .Map(dest => dest.Severity, src => src.Issue != null ? src.Issue.Severity.ToString() : "Unspecified");

        TypeAdapterConfig<Domain.Entities.Issue, IssueDetailsDto>
            .NewConfig()
            .Map(dest => dest.Latitude, src => src.Location != null ? src.Location.Y : 0)
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0)
            .Map(dest => dest.Category, src => src.Category.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Severity, src => src.Severity.ToString());

        TypeAdapterConfig<Domain.Entities.Issue, NearbyIssueDto>
            .NewConfig()
            .Map(dest => dest.Latitude, src => src.Location != null ? src.Location.Y : 0)
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0);
    }
}