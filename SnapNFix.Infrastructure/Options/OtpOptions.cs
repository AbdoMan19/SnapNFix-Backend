namespace SnapNFix.Infrastructure.Options;

public class OtpOptions
{
    public int OtpLifetimeMinutes { get; set; } = 5;
    
    public int OtpLength { get; set; } = 6;
    
    public int MinOtpValue { get; set; } = 100000;
    
    public int MaxOtpValue { get; set; } = 1000000;
    
    public int MaxVerificationAttempts { get; set; } = 3;
    
    public bool InvalidateOtpOnSuccessfulVerification { get; set; } = true;
    
    public int RateLimitWindowMinutes { get; set; } = 60;
    
    public int MaxRequestsPerWindow { get; set; } = 5;
}