using EduCheck.Core.Domain.Interfaces;
using EduCheck.Core.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace EduCheck.Infrastructure.Services;

public sealed class OllamaCodeReviewer(
    HttpClient httpClient,
    IConfiguration config,
    ILogger<OllamaCodeReviewer> logger) : IAiCodeReviewer
{
    private readonly string _model = config["AiSettings:ModelName"] ?? "codellama";

    public async Task<Result<string>> GetReviewAsync(string allCode, CancellationToken ct = default)
    {
        try
        {
            var request = new { model = _model, prompt = BuildPrompt(allCode), stream = false };
            var response = await httpClient.PostAsJsonAsync("api/generate", request, ct);

            if (!response.IsSuccessStatusCode)
                return Result.Failure<string>("AI.Error", "Ошибка API ИИ-анализа.");

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            return json.GetProperty("response").GetString() ?? "ИИ вернул пустой ответ.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ollama connection error");
            return Result.Failure<string>("AI.Unavailable", "Сервис ИИ недоступен.");
        }
    }

    private string BuildPrompt(string allCode)
    {
        return $"""
            Ты — опытный преподаватель программирования на C#. 
            Проанализируй следующий код студента. 
            Найди логические ошибки, плохие практики (bad smells) и предложи улучшения.
            Ответ дай на русском языке, кратко и по делу.
            
            КОД ДЛЯ АНАЛИЗА:
            {allCode}
            """;
    }
}