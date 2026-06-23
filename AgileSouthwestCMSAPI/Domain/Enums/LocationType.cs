namespace AgileSouthwestCMSAPI.Domain.Enums;

public enum LocationType
{
    Country = 1,
    StateProvince = 2,
    PostalCode = 3,
    Continent = 4,
    Radius = 5  // For distance-based shipping from warehouse
}