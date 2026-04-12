using EduCheck.Core.Contracts;
using EduCheck.Core.Interfaces;
using EduCheck.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;

namespace EduCheck.EmailWorker.Consumers;

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

        var history = await db.SubmissionHistory.FindAsync(historyId);
        if (history == null) return;

        var submission = await db.Submissions.FindAsync(history.SubmissionId);
        if (submission == null) return;

        logger.LogInformation($"[MQ] Начало анализа: {history.FileName}");

        try
        {
            using var stream = await storage.DownloadAsync(history.FileStoragePath);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            var roslynTask = codeAnalyzer.AnalyzeZipAsync(new MemoryStream(fileBytes));

            var codeForAi = await ExtractCodeForAiAsync(fileBytes);
            var aiTask = aiReviewer.GetReviewAsync(codeForAi);

            await Task.WhenAll(roslynTask, aiTask);

            var report = $"### ROSLYN ANALYSIS\n{roslynTask.Result}\n\n### AI REVIEW\n{aiTask.Result}";

            history.SetAnalysisResult(report);
            submission.CompleteAnalysis(report);

            await db.SaveChangesAsync();
            logger.LogInformation($"[MQ] Анализ завершен для {historyId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"[MQ] Ошибка анализа {historyId}");
        }
    }

    private async Task<string> ExtractCodeForAiAsync(byte[] zipBytes)
    {
        var sb = new StringBuilder();
        using var archive = new ZipArchive(new MemoryStream(zipBytes));
        foreach (var entry in archive.Entries.Where(e => e.Name.EndsWith(".cs")))
        {
            using var reader = new StreamReader(entry.Open());
            sb.AppendLine($"FILE: {entry.FullName}\n{await reader.ReadToEndAsync()}\n");
        }
        return sb.ToString();
    }
}
