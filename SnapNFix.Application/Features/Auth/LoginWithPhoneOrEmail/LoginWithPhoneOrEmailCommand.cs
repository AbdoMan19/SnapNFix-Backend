using MediatR;
using SnapNFix.Application.Common.ResponseModel;
namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public sealed record LoginWithPhoneOrEmailCommand(string PhoneOrEmail, string Password) : IRequest<GenericResponseModel<string>>;