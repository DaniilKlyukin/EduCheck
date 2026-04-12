using EduCheck.Application.Contracts;
using EduCheck.Application.Interfaces;
using EduCheck.Core.Domain.Entities;
using EduCheck.Core.Domain.Interfaces;
using EduCheck.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;

public class SubmissionAnalysisConsumer(
    IDbContextFactory<AppDbContext> dbFactory,
    IFileStorage storage,
    ICodeAnalyzer codeAnalyzer,
    IAiCodeReviewer aiReviewer,
    ILogger<SubmissionAnalysisConsumer> logger) : IConsumer<AnalyzeSubmissionTask>
{
    public async Task Consume(ConsumeContext<AnalyzeSubmissionTask> context)
    {
        var historyId = context.Message.HistoryId;
        using var db = await dbFactory.CreateDbContextAsync();

        var history = await db.Set<SubmissionHistory>().FirstOrDefaultAsync(h => h.Id == historyId);
        if (history == null) return;

        var submission = await db.Submissions.Include(s => s.History)
            .FirstOrDefaultAsync(s => s.Id == history.SubmissionId);
        if (submission == null) return;

        try
        {
            var downloadRes = await storage.DownloadAsync(history.File.StoragePath);
            if (downloadRes.IsFailure) return;

            using var ms = new MemoryStream();
            await downloadRes.Value.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var roslynTask = codeAnalyzer.AnalyzeZipAsync(new MemoryStream(fileBytes));
            var codeForAi = await ExtractCodeForAiAsync(fileBytes);
            var aiTask = aiReviewer.GetReviewAsync(codeForAi);

            await Task.WhenAll(roslynTask, aiTask);

            var report = $"### ROSLYN ANALYSIS\n{roslynTask.Result.Value}\n\n### AI REVIEW\n{aiTask.Result.Value}";

            var result = submission.CompleteAnalysis(historyId, report);
            if (result.IsSuccess)
            {
                await db.SaveChangesAsync();
                logger.LogInformation($"[MQ] Анализ завершен: {historyId}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[MQ] Критическая ошибка анализа {historyId}");
        }
    }

    private async Task<string> ExtractCodeForAiAsync(byte[] zipBytes)
    {
        var sb = new StringBuilder();
        using var archive = new ZipArchive(new MemoryStream(zipBytes));
        foreach (var entry in archive.Entries.Where(e => e.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            using var reader = new StreamReader(entry.Open());
            sb.AppendLine($"// FILE: {entry.FullName}");
            sb.AppendLine(await reader.ReadToEndAsync());
            sb.AppendLine();
        }
        return sb.ToString();
    }
}