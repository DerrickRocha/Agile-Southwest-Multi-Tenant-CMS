namespace AgileSouthwestCMSAPI.Utils;

public static class DistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;
    private const double EarthRadiusMiles = 3959.0;
    
    /// <summary>
    /// Calculates distance between two coordinates using Haversine formula
    /// </summary>
    public static double CalculateDistance(
        decimal lat1, decimal lon1,
        decimal lat2, decimal lon2,
        DistanceUnit unit = DistanceUnit.Kilometers)
    {
        var radius = unit == DistanceUnit.Miles ? EarthRadiusMiles : EarthRadiusKm;
        
        var dLat = ToRadians((double)(lat2 - lat1));
        var dLon = ToRadians((double)(lon2 - lon1));
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return radius * c;
    }
    
    private static double ToRadians(double degrees) => Math.PI * degrees / 180.0;
    
    public enum DistanceUnit
    {
        Kilometers,
        Miles
    }
}