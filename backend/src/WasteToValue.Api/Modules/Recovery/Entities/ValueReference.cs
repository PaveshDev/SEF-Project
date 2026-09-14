using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Entities;

public sealed class ValueReference
{
    public Guid Id { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public int Version { get; private set; } = 1;

    public void RequireVersion(int expected)
    {
        RecoveryRequestValidator.Version(expected);
        if (Version != expected) throw RecoveryException.Conflict("stale_version", "The resource changed. Reload before retrying.");
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
        Version = checked(Version + 1);
    }

    public Guid CategoryId { get; private set; }
    public ConditionGrade Condition { get; private set; }
    public RecoveryRoute Route { get; private set; }
    public decimal ValueLow { get; private set; }
    public decimal ValueHigh { get; private set; }
    public string Currency { get; private set; } = "";
    public string SourceName { get; private set; } = "";
    public string? SourceReference { get; private set; }
    public DateTimeOffset ObservedAt { get; private set; }
    public bool IsVerified { get; private set; }
    public Guid CreatedBy { get; private set; }
    private ValueReference() { }

    public static ValueReference Create(CreateValueReferenceRequest request, Guid actorId, DateTimeOffset now)
    {
        RecoveryRequestValidator.Id(actorId, "ActorId");
        RecoveryRequestValidator.Id(request.CategoryId, "CategoryId");
        RecoveryRequestValidator.Defined(request.Condition);
        RecoveryRequestValidator.Defined(request.Route);
        RecoveryRequestValidator.Money(request.ValueLow);
        RecoveryRequestValidator.Money(request.ValueHigh);
        if (request.ValueLow > request.ValueHigh) throw RecoveryException.Invalid("Minimum value must not exceed maximum value.");
        RecoveryRequestValidator.Currency(request.Currency);
        RecoveryRequestValidator.Text(request.SourceName, "SourceName", 200);
        if (request.SourceReference?.Length > 1000) throw RecoveryException.Invalid("SourceReference is too long.");
        if (request.ObservedAt == default || request.ObservedAt > now) throw RecoveryException.Invalid("ObservedAt must be a known past or current timestamp.");
        return new ValueReference
        {
            CategoryId = request.CategoryId, Condition = request.Condition, Route = request.Route,
            ValueLow = request.ValueLow, ValueHigh = request.ValueHigh, Currency = request.Currency,
            SourceName = request.SourceName.Trim(), SourceReference = request.SourceReference,
            ObservedAt = request.ObservedAt.ToUniversalTime(), CreatedBy = actorId, CreatedAt = now, UpdatedAt = now
        };
    }

    public void Verify(int expectedVersion, DateTimeOffset now)
    {
        RequireVersion(expectedVersion);
        if (IsVerified) throw RecoveryException.Conflict("already_verified", "The reference is already verified.");
        IsVerified = true;
        Touch(now);
    }

    public void Update(int expectedVersion, UpdateValueReferenceRequest request, DateTimeOffset now)
    {
        RequireVersion(expectedVersion);
        if (IsVerified) throw RecoveryException.Conflict("verified_reference_immutable", "Verified value references cannot be edited.");
        RecoveryRequestValidator.Defined(request.Condition);
        RecoveryRequestValidator.Defined(request.Route);
        RecoveryRequestValidator.Money(request.ValueLow);
        RecoveryRequestValidator.Money(request.ValueHigh);
        if (request.ValueLow > request.ValueHigh) throw RecoveryException.Invalid("Minimum value must not exceed maximum value.");
        RecoveryRequestValidator.Currency(request.Currency);
        RecoveryRequestValidator.Text(request.SourceName, "SourceName", 200);
        if (request.SourceReference?.Length > 1000) throw RecoveryException.Invalid("SourceReference is too long.");
        if (request.ObservedAt == default || request.ObservedAt > now) throw RecoveryException.Invalid("ObservedAt must be a known past or current timestamp.");
        Condition = request.Condition; Route = request.Route; ValueLow = request.ValueLow; ValueHigh = request.ValueHigh;
        Currency = request.Currency; SourceName = request.SourceName.Trim(); SourceReference = request.SourceReference;
        ObservedAt = request.ObservedAt.ToUniversalTime();
        Touch(now);
    }

    public ValueEvidence Snapshot() => new(Id, Version, ObservedAt, ValueLow, ValueHigh, Currency, SourceName);
}
