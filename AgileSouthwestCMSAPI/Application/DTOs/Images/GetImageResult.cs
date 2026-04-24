namespace AgileSouthwestCMSAPI.Application.DTOs.Images;

public record GetImageResult(int Id, int TenantId, string Url, string? FileName, string? ContentType, long? FileSize);