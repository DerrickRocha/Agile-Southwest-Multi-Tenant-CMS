namespace AgileSouthwestCMSAPI.Domain.Entities;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int OrderId { get; set; }
    public int ChangedBy { get; set; }

    public string NewStatus { get; set; }
    public string OldStatus { get; set; }
    public string NewPaymentStatus { get; set; }
    public string OldPaymentStatus { get; set; }
    public string NewFulfillmentStatus { get; set; }
    public string OldFulfillmentStatus { get; set; }
    public string Reason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime RowVersion { get; set; }  
    
    public Order Order { get; set; }
    public Tenant Tenant { get; set; }   
    public CmsUser ChangedByUser { get; set; }
    
}