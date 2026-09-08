using System.Globalization;
using System.Text.RegularExpressions;

namespace WasteToValue.Api.Modules.Recovery.Services;

// Not a record: generated ToString() must never expose credentials.
public sealed class GeminiRecoveryReasoningOptions
{
    internal string ApiKey { get; }
    internal string Model { get; }
    internal TimeSpan Timeout { get; }
    internal int MaxRetries { get; }

    private GeminiRecoveryReasoningOptions(string apiKey, string model, int timeoutSeconds, int maxRetries)
        => (ApiKey, Model, Timeout, MaxRetries) = (apiKey, model, TimeSpan.FromSeconds(timeoutSeconds), maxRetries);

    public static GeminiRecoveryReasoningOptions? FromConfiguration(IConfiguration configuration)
    {
        var key = configuration["Gemini:ApiKey"];
        var model = configuration["Gemini:Model"];
        if (!bool.TryParse(configuration["Gemini:Enabled"], out var enabled) || !enabled ||
            string.IsNullOrWhiteSpace(key) || key.Length > 256 ||
            key.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not '-' and not '_') ||
            string.IsNullOrWhiteSpace(model) || model.Length > 100 ||
            !Regex.IsMatch(model, "\\A[a-zA-Z0-9][a-zA-Z0-9._-]*\\z") ||
            !int.TryParse(configuration["Gemini:TimeoutSeconds"], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) ||
            seconds is < 1 or > 120 ||
            !int.TryParse(configuration["Gemini:MaxRetries"], NumberStyles.None, CultureInfo.InvariantCulture, out var retries) ||
            retries is < 0 or > 2)
            return null;
        return new(key, model, seconds, retries);
    }
}
