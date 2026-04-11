using EduCheck.Core.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SharpCompress.Archives;
using System.Collections.Immutable;
using System.Text;

namespace EduCheck.Infrastructure.Services;

public sealed class RoslynCodeAnalyzer : ICodeAnalyzer
{
    private static readonly ImmutableHashSet<string> ExcludedPathSegments =
        ImmutableHashSet.CreateRange(StringComparer.OrdinalIgnoreCase, ["obj", "bin", "Properties", ".vs", ".git"]);

    private static readonly string GlobalUsingsSource = @"
        global using global::System;
        global using global::System.Collections.Generic;
        global using global::System.IO;
        global using global::System.Linq;
        global using global::System.Net.Http;
        global using global::System.Threading;
        global using global::System.Threading.Tasks;
        global using global::System.Text;
    ";

    public async Task<string> AnalyzeZipAsync(Stream zipStream, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(zipStream);
        var syntaxTrees = new List<SyntaxTree>();

        try
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(GlobalUsingsSource, cancellationToken: ct));

            await using var archive = await ArchiveFactory.OpenAsyncArchive(zipStream);
            await foreach (var entry in archive.EntriesAsync.Where(e => !e.IsDirectory))
            {
                if (!IsEligibleSourceFile(entry.Key)) continue;

                using var entryStream = entry.OpenEntryStream();
                using var reader = new StreamReader(entryStream, Encoding.UTF8);

                var sourceCode = await reader.ReadToEndAsync(ct);
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceCode, path: entry.Key, cancellationToken: ct));
            }
        }
        catch (Exception ex)
        {
            return $"Ошибка обработки архива: {ex.Message}";
        }

        if (syntaxTrees.Count <= 1)
            return "Анализ пропущен: файлы C# не найдены.";

        return PerformCompilationAnalysis(syntaxTrees);
    }

    private string PerformCompilationAnalysis(IEnumerable<SyntaxTree> syntaxTrees)
    {
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication, // Поддержка Top-level statements
            optimizationLevel: OptimizationLevel.Release,
            warningLevel: 4,
            nullableContextOptions: NullableContextOptions.Enable,
            allowUnsafe: true
        );

        var compilation = CSharpCompilation.Create("EduCheck.Analysis")
            .WithOptions(compilationOptions)
            .AddReferences(GetFullFrameworkReferences())
            .AddSyntaxTrees(syntaxTrees);

        var diagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
            .Where(d => d.Location.SourceTree?.FilePath != "")
            .OrderByDescending(d => d.Severity)
            .ToImmutableList();

        return diagnostics.IsEmpty
            ? "Анализ завершен: Ошибок компиляции не обнаружено."
            : FormatDiagnosticsReport(diagnostics, syntaxTrees.Count() - 1);
    }

    private static IEnumerable<MetadataReference> GetFullFrameworkReferences()
    {
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (string.IsNullOrEmpty(assemblyPath)) return Array.Empty<MetadataReference>();

        var references = new List<MetadataReference>();

        string[] coreAssemblies = [
            "System.Runtime.dll",
            "mscorlib.dll",
            "System.Console.dll",
            "System.Collections.dll",
            "System.Linq.dll",
            "System.IO.dll",
            "System.Private.CoreLib.dll",
            "System.Net.Http.dll",
            "System.Drawing.Common.dll",
            "System.Windows.Forms.dll",
            "Microsoft.AspNetCore.Mvc.dll"
        ];

        foreach (var dll in coreAssemblies)
        {
            var path = Path.Combine(assemblyPath, dll);
            if (File.Exists(path))
                references.Add(MetadataReference.CreateFromFile(path));
        }

        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        return references.Concat(loadedAssemblies).DistinctBy(r => r.Display);
    }

    private bool IsEligibleSourceFile(string path)
    {
        if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) return false;
        var segments = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        return !segments.Any(ExcludedPathSegments.Contains);
    }

    private string FormatDiagnosticsReport(IReadOnlyList<Diagnostic> diagnostics, int filesCount)
    {
        var report = new StringBuilder();
        report.AppendLine("Отчет по анализу кода");
        report.AppendLine($"*Файлов проверено: {filesCount}*");
        report.AppendLine($"*Найдено проблем: {diagnostics.Count}*");
        report.AppendLine();

        foreach (var diag in diagnostics)
        {
            var lineSpan = diag.Location.GetLineSpan();
            var severity = diag.Severity == DiagnosticSeverity.Error ? "ERROR" : "WARN";

            report.AppendLine($"{severity} {diag.Id}: {diag.GetMessage()}");
            report.AppendLine($"  Файл: `{lineSpan.Path}`, Строка: {lineSpan.StartLinePosition.Line + 1}");
            report.AppendLine();
        }

        return report.ToString();
    }
}