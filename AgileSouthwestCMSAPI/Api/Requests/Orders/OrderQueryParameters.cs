namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public class OrderQueryParameters
{
    public string? OrderNumber { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? MinTotalCents { get; set; }
    public int? MaxTotalCents { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}