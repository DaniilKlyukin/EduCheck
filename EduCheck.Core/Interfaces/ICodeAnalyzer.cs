namespace EduCheck.Core.Interfaces;

public interface ICodeAnalyzer
{
    Task<string> AnalyzeZipAsync(Stream zipStream, CancellationToken ct = default);
}