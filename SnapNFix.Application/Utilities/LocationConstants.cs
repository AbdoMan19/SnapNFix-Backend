namespace SnapNFix.Application.Utilities;

public static class LocationConstants
{
    /// <summary>
    /// Egypt's geographical boundaries
    /// Source: Natural Earth Data / OpenStreetMap
    /// </summary>
    public static class Egypt
    {
        public const double MinLatitude = 22.0;  // ~Aswan/Sudan border
        public const double MaxLatitude = 31.7;  // ~Alexandria/Mediterranean Sea
        public const double MinLongitude = 25.0; // ~Siwa Oasis/Libya border  
        public const double MaxLongitude = 35.0; // ~Taba/Gaza Strip border
        
        public static readonly (double Lat, double Lng, string Name)[] MajorCities = 
        {
            (30.0444, 31.2357, "Cairo"),
            (31.2001, 29.9187, "Alexandria"), 
            (25.6872, 32.6396, "Aswan"),
            (26.8206, 30.8025, "Luxor"),
            (31.0409, 31.3785, "Port Said"),
            (27.1809, 31.1837, "Sharm El Sheikh")
        };
    }
}