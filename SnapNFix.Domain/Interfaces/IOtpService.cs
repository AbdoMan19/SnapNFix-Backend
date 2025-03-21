namespace SnapNFix.Domain.Interfaces;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string phoneNumber);
    Task<bool> VerifyOtpAsync(string phoneNumber, string otp);
    Task InvalidateOtpAsync(string phoneNumber);
}