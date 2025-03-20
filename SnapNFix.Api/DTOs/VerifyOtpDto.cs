namespace SnapNFix.Api.DTOs;

public class VerifyOtpDto
{
  public required string PhoneNumber { get; set; }
  public required string Otp { get; set; }
}