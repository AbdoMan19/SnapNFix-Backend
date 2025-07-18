using Mapster;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Features.Auth.Dtos; // Add this
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Features.FastReport.DTOs;

namespace SnapNFix.Application.Common.Mapping;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LoginWithPhoneOrEmailCommand, UserDevice>();

        // Add User to LoginResponse.UserInfo mapping
        TypeAdapterConfig<User, LoginResponse.UserInfo>
            .NewConfig()
            .Map(dest => dest.BirthDate, src => src.BirthDate)
            .Map(dest => dest.Gender, src => src.Gender);



        TypeAdapterConfig<Domain.Entities.FastReport, FastReportDetailsDto>
            .NewConfig()
            .Map(dest => dest.FirstName, src => src.User != null ? src.User.FirstName : string.Empty)
            .Map(dest => dest.LastName, src => src.User != null ? src.User.LastName : string.Empty);

        TypeAdapterConfig<Domain.Entities.SnapReport, ReportDetailsDto>
            .NewConfig()
            .Map(dest => dest.Latitude, src => src.Location != null ? src.Location.Y : 0)
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0)
            .Map(dest => dest.FirstName, src => src.User != null ? src.User.FirstName : string.Empty)
            .Map(dest => dest.LastName, src => src.User != null ? src.User.LastName : string.Empty)
            .Map(dest => dest.Status, src => src.ImageStatus)
            .Map(dest => dest.Category, src => src.ReportCategory);

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
            .Map(dest => dest.Severity, src => src.Severity.ToString())
            .Map(dest => dest.Images, src => src.AssociatedSnapReports
                .Where(sr => sr.ImageStatus == ImageStatus.Approved && !string.IsNullOrEmpty(sr.ImagePath))
                .OrderBy(sr => sr.CreatedAt)
                .Take(5)
                .Select(sr => sr.ImagePath)
                .ToList());

        TypeAdapterConfig<Domain.Entities.Issue, NearbyIssueDto>
            .NewConfig()
            .Map(dest => dest.Latitude, src => src.Location != null ? src.Location.Y : 0)
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0);
            
        config.NewConfig<HealthCondition, string>()
        .Map(dest => dest, src => src.ToString());
    }
}