using Microsoft.Extensions.Logging;
using SnapNFix.Application.Resources;
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
            return Shared.ValidLocation;
        }

        if (latitude < LocationConstants.Egypt.MinLatitude)
            return Shared.LocationTooFarSouth;

        if (latitude > LocationConstants.Egypt.MaxLatitude)
            return Shared.LocationTooFarNorth;

        if (longitude < LocationConstants.Egypt.MinLongitude)
            return Shared.LocationTooFarWest;

        if (longitude > LocationConstants.Egypt.MaxLongitude)
            return Shared.LocationTooFarEast;

        return Shared.LocationOutsideEgypt;
    }
}