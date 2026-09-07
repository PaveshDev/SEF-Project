using System.Reflection;
using WasteToValue.Api.Modules.Recovery.Validators;

internal static class Check
{
    private static int count;
    public static void That(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        count++;
    }
    public static void Error(Action action, string code)
    {
        try { action(); }
        catch (RecoveryException ex) { That(ex.Code == code, $"Expected {code}, got {ex.Code}."); return; }
        throw new Exception($"Expected {code}.");
    }
    public static async Task ErrorAsync(Func<Task> action, string code)
    {
        try { await action(); }
        catch (RecoveryException ex) { That(ex.Code == code, $"Expected {code}, got {ex.Code}."); return; }
        throw new Exception($"Expected {code}.");
    }
    public static void AssignId(object entity)
    {
        var property = entity.GetType().GetProperty("Id")!;
        if ((Guid)property.GetValue(entity)! == Guid.Empty) property.SetValue(entity, Guid.NewGuid());
    }
    public static void Finish() => Console.WriteLine($"PASS: {count} assertions across Recovery domain, services, contracts, DI, and EF metadata.");
}