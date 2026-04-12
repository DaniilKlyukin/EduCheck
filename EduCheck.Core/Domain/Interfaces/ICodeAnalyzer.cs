using EduCheck.Core.Primitives;

namespace EduCheck.Core.Domain.Interfaces;

public interface ICodeAnalyzer
{
    Task<Result<string>> AnalyzeZipAsync(Stream zipStream, CancellationToken ct = default);
}