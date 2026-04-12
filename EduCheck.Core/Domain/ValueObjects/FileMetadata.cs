using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.ValueObjects;

public record FileMetadata : ValueObject
{
    public string Name { get; }
    public string StoragePath { get; }
    public FileHash Hash { get; }

    public static Result<FileMetadata> Create(string name, string storagePath, FileHash hash)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<FileMetadata>("FileMetadataName.Empty", "Имя не может быть пустым.");

        if (string.IsNullOrWhiteSpace(storagePath))
            return Result.Failure<FileMetadata>("FileMetadataStoragePath.Empty", "Путь не может быть пустым.");

        return new FileMetadata(name, storagePath, hash);
    }

    private FileMetadata(string name, string storagePath, FileHash hash)
    {
        Name = name;
        StoragePath = storagePath;
        Hash = hash;
    }
}