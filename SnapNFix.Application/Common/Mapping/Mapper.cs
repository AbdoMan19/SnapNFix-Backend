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
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0);

        TypeAdapterConfig<Domain.Entities.SnapReport, IssueDetailsDto>
            .NewConfig()
            .Map(dest => dest.Latitude, src => src.Location != null ? src.Location.Y : 0)
            .Map(dest => dest.Longitude, src => src.Location != null ? src.Location.X : 0);
    }
}