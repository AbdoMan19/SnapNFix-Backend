using SnapNFix.Domain.Enums;

namespace SnapNFix.Domain.Interfaces;

public interface IOtpService
{
    public Task<string> GenerateOtpAsync(string emailOrPhoneNumber, OtpPurpose otpPurpose);
    public Task<bool> VerifyOtpAsync(string emailOrPhoneNumber, string otp, OtpPurpose purpose);
    public Task InvalidateOtpAsync(string emailOrPhoneNumber, OtpPurpose purpose);
}