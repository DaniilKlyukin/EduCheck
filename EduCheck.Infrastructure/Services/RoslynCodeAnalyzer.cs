using EduCheck.Core.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IO.Compression;
using System.Text;

namespace EduCheck.Infrastructure.Services;

public class RoslynCodeAnalyzer : ICodeAnalyzer
{
    public async Task<string> AnalyzeZipAsync(Stream zipStream, CancellationToken ct = default)
    {
        var report = new StringBuilder();
        var syntaxTrees = new List<SyntaxTree>();

        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);

        foreach (var entry in archive.Entries)
        {
            if (entry.FullName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(entry.Open());
                var code = await reader.ReadToEndAsync(ct);
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(code, cancellationToken: ct));
            }
        }

        if (syntaxTrees.Count == 0) return "Файлы .cs не найдены для анализа.";

        var compilation = CSharpCompilation.Create("Analysis")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(syntaxTrees);

        var diagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning)
            .ToList();

        if (diagnostics.Count == 0)
        {
            return "Анализ завершен: проблем не обнаружено. Код компилируется корректно.";
        }

        report.AppendLine($"Найдено проблем: {diagnostics.Count}");
        foreach (var diag in diagnostics)
        {
            report.AppendLine($"- [{diag.Id}] {diag.GetMessage()}");
            report.AppendLine($"  Место: {diag.Location.GetLineSpan().Path}, строка {diag.Location.GetLineSpan().StartLinePosition.Line}");
        }

        return report.ToString();
    }
}
