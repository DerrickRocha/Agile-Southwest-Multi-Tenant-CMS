namespace AgileSouthwestCMSAPI.Application.DTOs.S3;

public record S3ImageResult(string Url, string FileName, long FileSize, string ContentType);