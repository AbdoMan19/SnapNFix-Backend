using Microsoft.Extensions.Logging;
using SnapNFix.Application.Utilities;

namespace SnapNFix.Application.Common.Services.LocationValidation;

public class LocationValidationService : ILocationValidationService
{
    private readonly ILogger<LocationValidationService> _logger;

    public LocationValidationService(ILogger<LocationValidationService> logger)
    {
        _logger = logger;
    }

    public bool IsWithinEgypt(double latitude, double longitude)
    {
        var isValid = latitude >= LocationConstants.Egypt.MinLatitude && 
                     latitude <= LocationConstants.Egypt.MaxLatitude &&
                     longitude >= LocationConstants.Egypt.MinLongitude && 
                     longitude <= LocationConstants.Egypt.MaxLongitude;

        if (!isValid)
        {
            _logger.LogWarning("Location validation failed for coordinates: Lat={Latitude}, Lng={Longitude}", 
                latitude, longitude);
        }

        return isValid;
    }

    public string GetLocationValidationMessage(double latitude, double longitude)
    {
        if (IsWithinEgypt(latitude, longitude))
        {
            return "Location is valid";
        }

        if (latitude < LocationConstants.Egypt.MinLatitude)
            return "Location is too far south. Reports can only be created within Egypt's borders.";
        
        if (latitude > LocationConstants.Egypt.MaxLatitude)
            return "Location is too far north. Reports can only be created within Egypt's borders.";
        
        if (longitude < LocationConstants.Egypt.MinLongitude)
            return "Location is too far west. Reports can only be created within Egypt's borders.";
        
        if (longitude > LocationConstants.Egypt.MaxLongitude)
            return "Location is too far east. Reports can only be created within Egypt's borders.";

        return "Reports can only be created within Egypt's borders.";
    }
}