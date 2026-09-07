using System.Text.Json.Serialization;

namespace WasteToValue.Api.Modules.Recovery.DTOs.Integration;

[JsonConverter(typeof(JsonStringEnumConverter<GatewayOutcome>))]
public enum GatewayOutcome { Success, Unavailable, NotFound, Invalid, Stale }

public sealed record GatewayResult<T>
{
    public GatewayOutcome Outcome { get; }
    public T? Value { get; }
    public string Code { get; }
    public string Message { get; }
    public bool Retryable { get; }

    private GatewayResult(GatewayOutcome outcome, T? value, string code, string message, bool retryable)
        => (Outcome, Value, Code, Message, Retryable) = (outcome, value, code, message, retryable);

    public static GatewayResult<T> Success(T value) => new(GatewayOutcome.Success,
        value ?? throw new ArgumentNullException(nameof(value)), "ok", "", false);

    public static GatewayResult<T> Failure(GatewayOutcome outcome, string code, string message, bool retryable = false)
    {
        if (outcome == GatewayOutcome.Success || !Enum.IsDefined(outcome))
            throw new ArgumentOutOfRangeException(nameof(outcome));
        return new(outcome, default, code, message, retryable);
    }
}
