using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Interfaces;

public interface IOtpService : IScoped
{
    public Task<string> GenerateOtpAsync(string emailOrPhoneNumber, OtpPurpose otpPurpose);
    public Task<bool> VerifyOtpAsync(string emailOrPhoneNumber, string otp, OtpPurpose purpose);
    public Task InvalidateOtpAsync(string emailOrPhoneNumber, OtpPurpose purpose);
}


