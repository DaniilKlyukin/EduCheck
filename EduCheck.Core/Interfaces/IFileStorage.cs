namespace EduCheck.Core.Interfaces;

public interface IFileStorage
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string objectName, CancellationToken ct = default);
    Task DeleteAsync(string objectName, CancellationToken ct = default);
}