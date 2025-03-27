using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Commands.PhoneVerification;

public class PhoneVerificationCommandHandler(ITokenService tokenService , IUnitOfWork unitOfWork , UserManager<User> userManager , IOtpService otpService) 
    : IRequestHandler<PhoneVerificationCommand , GenericResponseModel<AuthResponse>>
{
    public async Task<GenericResponseModel<AuthResponse>> Handle(PhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        var user =  await unitOfWork.Repository<User>()
            .FindBy(u => u.PhoneNumber == request.PhoneNumber)
            .FirstOrDefaultAsync();
        if (user is null)
        {
            return GenericResponseModel<AuthResponse>.Failure("User not found");
        }

        var valid = await otpService.VerifyOtpAsync(request.PhoneNumber, request.Otp);
        if (valid)
        {
            var token = await tokenService.GenerateJwtToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();
            return GenericResponseModel<AuthResponse>.Success(new AuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = tokenService.GetTokenExpiration()
            });
        }

        return GenericResponseModel<AuthResponse>.Failure("Otp Verification Failed");
    }
}