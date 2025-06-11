using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Common.Services.LocationValidation;

public interface ILocationValidationService : IScoped
{
    bool IsWithinEgypt(double latitude, double longitude);
    string GetLocationValidationMessage(double latitude, double longitude);
}