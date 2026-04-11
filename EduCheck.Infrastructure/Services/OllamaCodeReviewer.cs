using EduCheck.Core.Interfaces;
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

    public async Task<string> GetReviewAsync(string allCode, CancellationToken ct = default)
    {
        var prompt = $"""
            Ты — опытный преподаватель программирования на C#. 
            Проанализируй следующий код студента. 
            Найди логические ошибки, плохие практики (bad smells) и предложи улучшения.
            Ответ дай на русском языке, кратко и по делу.
            
            КОД ДЛЯ АНАЛИЗА:
            {allCode}
            """;

        try
        {
            var request = new
            {
                model = _model,
                prompt,
                stream = false
            };

            var response = await httpClient.PostAsJsonAsync("api/generate", request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
            return json.GetProperty("response").GetString() ?? "ИИ не смог подготовить ответ.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обращении к Ollama");
            return $"[Ошибка ИИ-анализа]: {ex.Message}";
        }
    }
}