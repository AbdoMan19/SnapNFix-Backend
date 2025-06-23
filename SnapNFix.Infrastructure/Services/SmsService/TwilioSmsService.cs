using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SnapNFix.Infrastructure.Services.SmsService;

public class TwilioSmsService : ISmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly bool _isEnabled;

    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
      _accountSid = configuration["Twilio:AccountSid"];
      _authToken = configuration["Twilio:AuthToken"];
      _fromNumber = configuration["Twilio:FromNumber"];
      _isEnabled = bool.TryParse(configuration["Twilio:Enabled"], out bool enabled) && enabled;
      _logger = logger;
      
      if (_isEnabled)
      {
          TwilioClient.Init(_accountSid, _authToken);
      }
    }

  public async Task<bool> SendSmsAsync(string phoneNumber, string message)
  {
    if (!_isEnabled)
    {
      _logger.LogWarning("SMS service is disabled. Would have sent message to {PhoneNumber}: {Message}", phoneNumber, message);
      return false;
    }
    
    try
    {
      var messageResource = await MessageResource.CreateAsync(
        body: message,
        from: new PhoneNumber(_fromNumber),
        to: new PhoneNumber(phoneNumber)
      );
      _logger.LogInformation("SMS sent to {PhoneNumber}, Message SID: {MessageSid}", phoneNumber, messageResource.Sid);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}: {ErrorMessage}", phoneNumber, ex.Message);
      return false;
    }
  }
}
