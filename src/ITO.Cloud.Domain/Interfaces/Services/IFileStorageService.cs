namespace ITO.Cloud.Domain.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string filePath, int expiryMinutes = 60, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);
}
