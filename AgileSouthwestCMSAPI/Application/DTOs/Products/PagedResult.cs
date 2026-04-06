namespace AgileSouthwestCMSAPI.Application.DTOs.Products;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);