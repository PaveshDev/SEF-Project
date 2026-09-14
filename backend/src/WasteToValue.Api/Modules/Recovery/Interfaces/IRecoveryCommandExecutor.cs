using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public sealed record RecoveryCommand(Guid ActorId, string Operation, string Key, string RequestHash)
{
    public static RecoveryCommand Create(RecoveryActor actor, string operation, string key, object request)
    {
        RecoveryRequestValidator.Id(actor.UserId, "ActorId");
        RecoveryRequestValidator.Text(key, "Idempotency-Key", 100);
        return new(actor.UserId, operation, key,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request, request.GetType())))));
    }

    // Stable downstream operation identity for replay of the same accepted command.
    public Guid ChildOperation(string step)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{ActorId:D}|{Operation}|{Key}|{RequestHash}|{step}"));
        return new Guid(bytes.AsSpan(0, 16));
    }
}

// Must persist receipts, reject key/hash conflicts, serialize duplicate requests,
// invoke authorize before EVERY receipt lookup, and atomically commit callback writes
// + response using the existing scoped AppDbContext. Begin the transaction before
// authorization reads and do not reuse entities tracked before that transaction.
// Approval callbacks require serializable, cross-module freshness validation.
// Matching/pickup adapters must enlist their database work in the same unit of work.
// Do not retry callbacks with externally committed side effects without an agreed
// durable continuation protocol. Never use an in-memory implementation in production.
public interface IRecoveryCommandExecutor
{
    Task<T> ExecuteAsync<T>(RecoveryCommand command, Func<CancellationToken, Task> authorize,
        Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken);
}
