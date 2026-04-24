namespace AgileSouthwestCMSAPI.Api.Requests.ProductImages;

public record ReorderImagesRequest(
    int ProductId,
    int[] ImageOrder, 
    int? PrimaryImageId = null
);