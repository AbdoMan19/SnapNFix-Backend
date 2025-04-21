using Mapster;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Common.Mapping;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LoginWithPhoneOrEmailCommand, UserDevice>();
    }
}