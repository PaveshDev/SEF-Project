using System.Net.Http.Json;
using System.Text.Json;
using LoopWorth.Application.Common.Interfaces;
using LoopWorth.Domain.Entities;
using LoopWorth.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LoopWorth.Infrastructure.Agents;

public class ItemAssessmentAgent : IItemAssessmentAgent
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ItemAssessmentAgent> _logger;

    public ItemAssessmentAgent(HttpClient httpClient, IConfiguration configuration, ILogger<ItemAssessmentAgent> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GEMINI_ITEMS_API_KEY"] ?? Environment.GetEnvironmentVariable("GEMINI_ITEMS_API_KEY") ?? string.Empty;
        _logger = logger;
    }

    public async Task<ItemAssessmentResult> AssessItemAsync(Item item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("GEMINI_ITEMS_API_KEY is missing.");
            throw new InvalidOperationException("Gemini API key is missing.");
        }

        var prompt = $@"
You are the Item Assessment Agent for LoopWorth.
Analyze the following item and recommend a recovery route (Reuse, Donate, or Recycle).

Item Information:
ID: {item.Id}
Name: {item.Name}
Category: {item.Category?.Name ?? "Unknown"}
Brand: {item.Brand ?? "Unknown"}
Model: {item.Model ?? "Unknown"}
Condition Description: {item.ConditionDescription ?? "Unknown"}

Output JSON ONLY with the exact following schema:
{{
  ""conditionLevel"": ""Good"" | ""Fair"" | ""Poor"" | ""Unknown"",
  ""recommendedRoute"": ""Reuse"" | ""Donate"" | ""Recycle"",
  ""alternativeRoute"": ""Reuse"" | ""Donate"" | ""Recycle"" | null,
  ""confidenceLevel"": ""Low"" | ""Medium"" | ""High"",
  ""explanation"": ""Reason for the recommendation""
}}
";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
        
        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Gemini API failed: {Error}", error);
            throw new Exception("Assessment could not be completed.");
        }

        var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        
        try
        {
            var textResult = jsonResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (textResult == null) throw new Exception("Empty response from Gemini.");

            // Basic cleanup to extract JSON in case of markdown formatting
            var jsonString = textResult.Trim();
            if (jsonString.StartsWith("```json"))
            {
                jsonString = jsonString.Substring(7);
                if (jsonString.EndsWith("```")) jsonString = jsonString.Substring(0, jsonString.Length - 3);
            }
            else if (jsonString.StartsWith("```"))
            {
                jsonString = jsonString.Substring(3);
                if (jsonString.EndsWith("```")) jsonString = jsonString.Substring(0, jsonString.Length - 3);
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<ItemAssessmentResult>(jsonString, options);

            if (result == null) throw new Exception("Deserialized result is null.");

            // Validation step
            if (!Enum.IsDefined(typeof(ConditionLevel), result.ConditionLevel) ||
                !Enum.IsDefined(typeof(RecoveryRoute), result.RecommendedRoute) ||
                !Enum.IsDefined(typeof(ConfidenceLevel), result.ConfidenceLevel))
            {
                throw new Exception("Gemini returned invalid enum values.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response.");
            throw new Exception("Assessment could not be completed.");
        }
    }
}
