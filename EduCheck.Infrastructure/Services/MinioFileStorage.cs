using EduCheck.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace EduCheck.Infrastructure.Services;

public class MinioFileStorage : IFileStorage
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public MinioFileStorage(IMinioClient minioClient, IConfiguration config)
    {
        _minioClient = minioClient;
        _bucketName = config["StorageSettings:BucketName"] ?? "submissions";
    }

    public async Task DeleteAsync(string objectName, CancellationToken ct = default)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName);

        await _minioClient.RemoveObjectAsync(removeObjectArgs, ct);
    }

    public async Task<Stream> DownloadAsync(string objectName, CancellationToken ct = default)
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

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
        var found = await _minioClient.BucketExistsAsync(beArgs, ct);

        if (!found)
        {
            var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
            await _minioClient.MakeBucketAsync(mbArgs, ct);
        }

        var objectName = $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}_{fileName}";

        stream.Position = 0;
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, ct);

        return objectName;
    }
}
