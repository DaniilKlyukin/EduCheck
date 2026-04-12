using EduCheck.Application.Interfaces;
using EduCheck.Core.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace EduCheck.Infrastructure.Services;

public class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioFileStorage> _logger;
    private readonly string _bucketName;

    public MinioFileStorage(
        IMinioClient minioClient, 
        IConfiguration config,
        ILogger<MinioFileStorage> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
        _bucketName = config["StorageSettings:BucketName"] ?? "submissions";
    }

    public async Task<Result> DeleteAsync(string objectName, CancellationToken ct = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, ct);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Не удалось удалить файл.");
            return Result.Failure<Stream>("Storage.DownloadError", "Не удалось удалить файл.");
        }
    }

    public async Task<Result<Stream>> DownloadAsync(string objectName, CancellationToken ct = default)
    {
        try
        {
            var ms = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream(s => s.CopyTo(ms));

            await _minioClient.GetObjectAsync(getObjectArgs, ct);

            ms.Position = 0;

            return ms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Не удалось скачать файл.");
            return Result.Failure<Stream>("Storage.DownloadError", "Не удалось скачать файл.");
        }
    }

    public async Task<Result<string>> GetDownloadUrlAsync(string path, string fileName)
    {
        try
        {
            int expirySeconds = (int)TimeSpan.FromHours(1).TotalSeconds;

            var args = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(path)
                .WithExpiry(expirySeconds)
                .WithHeaders(new Dictionary<string, string>
                {
                { "response-content-disposition", $"attachment; filename=\"{fileName}\"" }
                });

            return await _minioClient.PresignedGetObjectAsync(args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка генерации ссылки для файла {path}");
            return Result.Failure<string>("Storage.DownloadError", $"Ошибка генерации ссылки для файла {path}");
        }
    }

    public async Task<Result<string>> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        try
        {
            var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
            if (!await _minioClient.BucketExistsAsync(beArgs, ct))
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName), ct);
            }

            var objectName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";
            stream.Position = 0;

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType), ct);

            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Minio Upload Error");
            return Result.Failure<string>("Storage.UploadError", "Не удалось загрузить файл.");
        }
    }
}