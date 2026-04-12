using EduCheck.Core.Primitives;

namespace EduCheck.Application.Interfaces;

public interface IFileStorage
{
    Task<Result<string>> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<Result<Stream>> DownloadAsync(string objectName, CancellationToken ct = default);
    Task<Result> DeleteAsync(string objectName, CancellationToken ct = default);
    Task<Result<string>> GetDownloadUrlAsync(string path, string fileName);
}