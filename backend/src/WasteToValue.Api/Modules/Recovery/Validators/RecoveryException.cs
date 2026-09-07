namespace WasteToValue.Api.Modules.Recovery.Validators;

public sealed class RecoveryException(int status, string code, string message) : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public static RecoveryException Invalid(string message) => new(400, "invalid_input", message);
    public static RecoveryException Conflict(string code, string message) => new(409, code, message);
    public static RecoveryException Unavailable(string code, string message) => new(503, code, message);
}
