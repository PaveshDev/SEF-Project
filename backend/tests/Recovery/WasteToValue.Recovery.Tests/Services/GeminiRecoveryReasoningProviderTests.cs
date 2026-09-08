using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WasteToValue.Api.Modules.Recovery;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.DTOs.Reasoning;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;

namespace WasteToValue.Recovery.Tests.Services;

public sealed class GeminiRecoveryReasoningProviderTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task Valid_response_ranks_identifiers_and_uses_a_closed_nonfinancial_schema()
    {
        var request = Request();
        var expected = Recommendation(request);
        using var handler = new FakeHttp((_, _) => Task.FromResult(Envelope(expected)));
        using var http = new HttpClient(handler);
        var result = await Provider(http).ReasonAsync(request, default);
        Assert.Equal(GatewayOutcome.Success, result.Outcome);
        Assert.Equal(expected.RankedOptionIds, result.Value!.RankedOptionIds);
        Assert.True(result.Value.RequiresHumanReview);
        Assert.Equal(1, handler.Calls);
        Assert.Equal("generativelanguage.googleapis.com", handler.Uri!.Host);
        Assert.Empty(handler.Uri.Query);
        using var body = JsonDocument.Parse(handler.Body!);
        Assert.False(body.RootElement.TryGetProperty("tools", out _));
        var schema = body.RootElement.GetProperty("generationConfig").GetProperty("responseJsonSchema");
        Assert.False(schema.GetProperty("additionalProperties").GetBoolean());
        Assert.Equal(10, schema.GetProperty("properties").EnumerateObject().Count());
        Assert.DoesNotContain("netValue", schema.GetRawText());
        Assert.DoesNotContain("apiKey", handler.Body);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not JSON")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("[]")]
    [InlineData("```json\n{}\n```")]
    public async Task Malformed_empty_or_incomplete_output_is_rejected_without_retry(string text)
    {
        using var handler = new FakeHttp((_, _) => Task.FromResult(EnvelopeText(text)));
        using var http = new HttpClient(handler);
        var result = await Provider(http).ReasonAsync(Request(), default);
        Assert.Equal(GatewayOutcome.Invalid, result.Outcome);
        Assert.Null(result.Value);
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("recommendedRoute", "\"InventedRoute\"")]
    [InlineData("recommendedRoute", "0")]
    [InlineData("rankedOptionIds", "[]")]
    [InlineData("confidence", "2")]
    [InlineData("confidence", "null")]
    [InlineData("reasonSummary", "null")]
    [InlineData("benefits", "null")]
    [InlineData("evidenceReferences", "[\"invented evidence\"]")]
    [InlineData("netValue", "999999")]
    [InlineData("currency", "\"USD\"")]
    [InlineData("hiddenReasoning", "\"private deliberation\"")]
    [InlineData("tool", "\"approveProposal\"")]
    public async Task Invalid_or_extra_fields_are_rejected(string field, string value)
    {
        var request = Request();
        var node = JsonSerializer.SerializeToNode(Recommendation(request), Json)!.AsObject();
        node[field] = JsonNode.Parse(value);
        using var handler = new FakeHttp((_, _) => Task.FromResult(EnvelopeText(node.ToJsonString())));
        using var http = new HttpClient(handler);
        var result = await Provider(http).ReasonAsync(request, default);
        Assert.Equal(GatewayOutcome.Invalid, result.Outcome);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Duplicate_properties_are_rejected()
    {
        var request = Request();
        var text = JsonSerializer.Serialize(Recommendation(request), Json);
        text = text.Insert(1, "\"confidence\":0.1,");
        using var handler = new FakeHttp((_, _) => Task.FromResult(EnvelopeText(text)));
        using var http = new HttpClient(handler);
        Assert.Equal(GatewayOutcome.Invalid, (await Provider(http).ReasonAsync(request, default)).Outcome);
    }

    [Theory]
    [InlineData("MAX_TOKENS")]
    [InlineData("SAFETY")]
    public async Task Incomplete_or_blocked_generation_is_not_accepted(string finish)
    {
        using var handler = new FakeHttp((_, _) => Task.FromResult(EnvelopeText("{}", finish)));
        using var http = new HttpClient(handler);
        Assert.Equal(GatewayOutcome.Invalid, (await Provider(http).ReasonAsync(Request(), default)).Outcome);
    }

    [Fact]
    public async Task Function_call_or_thought_parts_are_never_consumed()
    {
        var request = Request();
        foreach (var extra in new[] { "\"functionCall\":{\"name\":\"approveProposal\"}", "\"thought\":true" })
        {
            var text = JsonSerializer.Serialize(JsonSerializer.Serialize(Recommendation(request), Json));
            var envelope = "{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"text\":" + text + "," + extra + "}]}}]}";
            using var handler = new FakeHttp((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent(envelope) }));
            using var http = new HttpClient(handler);
            Assert.Equal(GatewayOutcome.Invalid, (await Provider(http).ReasonAsync(request, default)).Outcome);
        }
    }

    [Theory]
    [InlineData(408)]
    [InlineData(429)]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    public async Task Transient_statuses_use_bounded_retries(int status)
    {
        using var handler = new FakeHttp((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)));
        using var http = new HttpClient(handler);
        var result = await Provider(http).ReasonAsync(Request(), default);
        Assert.Equal(GatewayOutcome.Unavailable, result.Outcome);
        Assert.Equal(3, handler.Calls);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(404)]
    [InlineData(302)]
    [InlineData(501)]
    public async Task Authentication_validation_redirect_and_permanent_errors_are_not_retried(int status)
    {
        using var handler = new FakeHttp((_, _) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status)
            { Content = new StringContent("sensitive upstream response") }));
        using var http = new HttpClient(handler);
        var result = await Provider(http).ReasonAsync(Request(), default);
        Assert.NotEqual(GatewayOutcome.Success, result.Outcome);
        Assert.Equal(1, handler.Calls);
        Assert.DoesNotContain("sensitive", result.Message);
    }

    [Fact]
    public async Task Transient_failure_can_recover_and_transport_retries_are_bounded()
    {
        var request = Request();
        var calls = 0;
        using var handler = new FakeHttp((_, _) => ++calls < 3
            ? throw new HttpRequestException(HttpRequestError.ConnectionError)
            : Task.FromResult(Envelope(Recommendation(request))));
        using var http = new HttpClient(handler);
        Assert.Equal(GatewayOutcome.Success, (await Provider(http).ReasonAsync(request, default)).Outcome);
        Assert.Equal(3, handler.Calls);
    }

    [Fact]
    public async Task Timeout_is_safe_and_cancels_the_http_operation()
    {
        var cancelled = false;
        using var handler = new FakeHttp(async (_, ct) =>
        {
            try { await Task.Delay(Timeout.InfiniteTimeSpan, ct); }
            catch (OperationCanceledException) { cancelled = true; throw; }
            throw new InvalidOperationException();
        });
        using var http = new HttpClient(handler);
        var result = await Provider(http, timeout: "1").ReasonAsync(Request(), default);
        Assert.Equal("reasoning_timeout", result.Code);
        Assert.True(cancelled);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task Caller_cancellation_is_propagated_before_and_during_http()
    {
        using var cts = new CancellationTokenSource();
        using var handler = new FakeHttp(async (_, ct) =>
        {
            cts.Cancel();
            await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            throw new InvalidOperationException();
        });
        using var http = new HttpClient(handler);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Provider(http).ReasonAsync(Request(), cts.Token));
        Assert.Equal(1, handler.Calls);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Provider(http).ReasonAsync(Request(), cts.Token));
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Oversized_responses_are_rejected_with_or_without_content_length(bool length)
    {
        using var handler = new FakeHttp((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = length ? new StringContent(new string('x', 65537)) :
                new StreamContent(new UnseekableStream(new byte[65537]))
        }));
        using var http = new HttpClient(handler);
        Assert.Equal("reasoning_response_too_large", (await Provider(http).ReasonAsync(Request(), default)).Code);
        Assert.Equal(1, handler.Calls);
    }

    [Theory]
    [InlineData("Gemini:ApiKey", null)]
    [InlineData("Gemini:Enabled", "false")]
    [InlineData("Gemini:Enabled", null)]
    [InlineData("Gemini:Model", null)]
    [InlineData("Gemini:Model", "../arbitrary-url")]
    [InlineData("Gemini:TimeoutSeconds", "0")]
    [InlineData("Gemini:TimeoutSeconds", "121")]
    [InlineData("Gemini:TimeoutSeconds", null)]
    [InlineData("Gemini:MaxRetries", "3")]
    [InlineData("Gemini:MaxRetries", "-1")]
    [InlineData("Gemini:MaxRetries", null)]
    public async Task Missing_or_invalid_configuration_selects_unavailable(string name, string? value)
    {
        var values = Settings();
        values[name] = value;
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        services.AddRecoveryModule();
        using var container = services.BuildServiceProvider();
        using var scope = container.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IRecoveryReasoningProvider>();
        Assert.IsType<UnavailableRecoveryReasoningProvider>(provider);
        var result = await provider.ReasonAsync(Request(), default);
        Assert.Equal("IntegrationUnavailable", result.Code);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.ReasonAsync(Request(), cancelled.Token));
    }

    [Fact]
    public void Valid_configuration_selects_Gemini_and_fake_registration_is_preserved()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(Settings()).Build());
        services.AddRecoveryModule();
        using var container = services.BuildServiceProvider();
        using var scope = container.CreateScope();
        // Resolve only. Never invoke an HTTP client with a real transport in automated tests.
        Assert.IsType<GeminiRecoveryReasoningProvider>(scope.ServiceProvider.GetRequiredService<IRecoveryReasoningProvider>());
        var fakeServices = new ServiceCollection();
        var fake = new UnavailableRecoveryReasoningProvider();
        fakeServices.AddSingleton<IRecoveryReasoningProvider>(fake);
        fakeServices.AddRecoveryModule();
        using var fakeContainer = fakeServices.BuildServiceProvider();
        Assert.Same(fake, fakeContainer.GetRequiredService<IRecoveryReasoningProvider>());
    }

    private static Dictionary<string, string?> Settings() => new()
    {
        // Synthetic test-only configuration. No user secrets or environment providers are loaded.
        ["Gemini:ApiKey"] = Guid.NewGuid().ToString("N"),
        ["Gemini:Model"] = "test-model",
        ["Gemini:Enabled"] = bool.TrueString,
        ["Gemini:TimeoutSeconds"] = "10",
        ["Gemini:MaxRetries"] = "2"
    };

    private static GeminiRecoveryReasoningProvider Provider(HttpClient http, string timeout = "10")
    {
        var values = Settings();
        values["Gemini:TimeoutSeconds"] = timeout;
        return new(http, GeminiRecoveryReasoningOptions.FromConfiguration(
            new ConfigurationBuilder().AddInMemoryCollection(values).Build())!, new RecoveryReasoningValidator());
    }

    internal static RecoveryReasoningRequest Request() => new(Guid.NewGuid(), "Furniture",
        new(FunctionalStatus.Working, new[] { "assessment-reference" }), ConditionGrade.Good,
        new[]
        {
            new RecoveryReasoningOption(Guid.NewGuid(), RecoveryRoute.Reuse,
                new(100, 200, 0, 0, 100, "LKR"), new[] { "value-reference" }, null, null),
            new RecoveryReasoningOption(Guid.NewGuid(), RecoveryRoute.Donate,
                new(0, 0, 0, 0, 0, "LKR"), new[] { "value-reference" },
                new("Eligible", "Accepted"), new("Feasible", 0, "LKR"))
        }, new("Keep the item useful", null, "LKR", null));

    internal static RecoveryReasoningResponse Recommendation(RecoveryReasoningRequest request) =>
        new(request.EligibleOptions.Last().Route, request.EligibleOptions.Reverse().Select(x => x.OptionId).ToArray(),
            "Supplied evidence supports this option.", new[] { "Continued use" }, Array.Empty<string>(),
            Array.Empty<string>(), new[] { request.EligibleOptions[0].EvidenceReferences[0] },
            Array.Empty<string>(), 0.8, false);

    private static HttpResponseMessage Envelope(RecoveryReasoningResponse response) =>
        EnvelopeText(JsonSerializer.Serialize(response, Json));
    private static HttpResponseMessage EnvelopeText(string text, string finish = "STOP") => new(HttpStatusCode.OK)
    {
        Content = new StringContent(JsonSerializer.Serialize(new
        {
            candidates = new[] { new { finishReason = finish, content = new { parts = new[] { new { text } } } } }
        }), Encoding.UTF8, "application/json")
    };

    // The entire transport terminates here: no socket, DNS lookup or real Gemini call.
    private sealed class FakeHttp(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public Uri? Uri { get; private set; }
        public string? Body { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls++;
            Uri = request.RequestUri;
            Body = await request.Content!.ReadAsStringAsync(ct);
            return await respond(request, ct);
        }
    }

    private sealed class UnseekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
    }
}
