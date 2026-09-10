using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Api.Modules.Recovery.Entities;

public sealed class RecoveryCase
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

    public Guid ItemId { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid AssessmentId { get; private set; }
    public int ItemRevision { get; private set; }
    public int AssessmentVersion { get; private set; }
    public int Revision { get; private set; } = 1;
    public string Objective { get; private set; } = "";
    public string PreferredRoutesJson { get; private set; } = "[]";
    public IReadOnlyList<RecoveryRoute> PreferredRoutes => JsonSerializer.Deserialize<RecoveryRoute[]>(PreferredRoutesJson)!;
    public string Currency { get; private set; } = "";
    public decimal? MaximumPickupCost { get; private set; }
    public DateTimeOffset? Deadline { get; private set; }
    public RecoveryCaseStatus Status { get; private set; } = RecoveryCaseStatus.Draft;
    public bool IsActive => Status is not (RecoveryCaseStatus.Rejected or RecoveryCaseStatus.Completed or RecoveryCaseStatus.Failed or RecoveryCaseStatus.Cancelled);
    private RecoveryCase() { }

    public static RecoveryCase Create(Guid ownerId, Guid itemId, AssessmentSummary assessment, RecoveryInputs inputs, DateTimeOffset now)
    {
        RecoveryRequestValidator.Id(ownerId, "OwnerId");
        RecoveryRequestValidator.Id(itemId, "ItemId");
        RecoveryRequestValidator.Assessment(assessment, itemId, ownerId);
        RecoveryRequestValidator.Inputs(inputs, now);
        var result = new RecoveryCase { OwnerId = ownerId, ItemId = itemId, CreatedAt = now, UpdatedAt = now };
        result.SetInputs(inputs, assessment);
        return result;
    }

    public void UpdateInputs(int expectedVersion, RecoveryInputs inputs, AssessmentSummary assessment, DateTimeOffset now)
    {
        RequireVersion(expectedVersion);
        RequireState(RecoveryCaseStatus.Draft, RecoveryCaseStatus.RevisionRequested,
            RecoveryCaseStatus.Planning, RecoveryCaseStatus.AwaitingInputs, RecoveryCaseStatus.AwaitingApproval);
        RecoveryRequestValidator.Inputs(inputs, now);
        RecoveryRequestValidator.Assessment(assessment, ItemId, OwnerId);
        SetInputs(inputs, assessment);
        if (Status != RecoveryCaseStatus.Draft) Status = RecoveryCaseStatus.RevisionRequested;
        Revision = checked(Revision + 1);
        Touch(now);
    }

    public void RequireAssessment(AssessmentSummary assessment)
    {
        RecoveryRequestValidator.Assessment(assessment, ItemId, OwnerId);
        if (AssessmentId != assessment.AssessmentId || AssessmentVersion != assessment.AssessmentVersion || ItemRevision != assessment.ItemRevision)
            throw RecoveryException.Conflict("stale_assessment", "The confirmed assessment changed. Revise the case inputs.");
    }

    public void StartPlanning(int expectedVersion, DateTimeOffset now)
    {
        RequireVersion(expectedVersion);
        RequireState(RecoveryCaseStatus.Draft, RecoveryCaseStatus.RevisionRequested, RecoveryCaseStatus.AwaitingInputs, RecoveryCaseStatus.Failed);
        if (Deadline <= now) throw RecoveryException.Conflict("deadline_passed", "The recovery deadline has passed.");
        Status = RecoveryCaseStatus.Planning;
        Touch(now);
    }

    public void PrepareReplan(int expectedVersion, DateTimeOffset now)
    {
        RequireVersion(expectedVersion);
        RequireState(RecoveryCaseStatus.Planning, RecoveryCaseStatus.AwaitingApproval,
            RecoveryCaseStatus.RevisionRequested, RecoveryCaseStatus.AwaitingInputs, RecoveryCaseStatus.Failed);
        if (Deadline <= now) throw RecoveryException.Conflict("deadline_passed", "The recovery deadline has passed.");
        Revision = checked(Revision + 1);
        Status = RecoveryCaseStatus.RevisionRequested;
        Touch(now);
    }

    public void AwaitInputs(DateTimeOffset now) => Move(RecoveryCaseStatus.AwaitingInputs, now, RecoveryCaseStatus.Planning);
    public void Submit(DateTimeOffset now) => Move(RecoveryCaseStatus.AwaitingApproval, now, RecoveryCaseStatus.Planning);
    public void Fail(DateTimeOffset now) => Move(RecoveryCaseStatus.Failed, now, RecoveryCaseStatus.Planning);
    public void RequireRevision(DateTimeOffset now) => Move(RecoveryCaseStatus.RevisionRequested, now, RecoveryCaseStatus.AwaitingApproval);
    public void Complete(DateTimeOffset now) => Move(RecoveryCaseStatus.Completed, now, RecoveryCaseStatus.Approved);

    public void Decide(ProposalDecisionKind decision, DateTimeOffset now)
    {
        RecoveryRequestValidator.Defined(decision);
        Move(decision switch
        {
            ProposalDecisionKind.Approved => RecoveryCaseStatus.Approved,
            ProposalDecisionKind.Rejected => RecoveryCaseStatus.Rejected,
            _ => RecoveryCaseStatus.RevisionRequested
        }, now, RecoveryCaseStatus.AwaitingApproval);
    }

    public void Cancel(int expectedVersion, DateTimeOffset now)
    {
        RequireVersion(expectedVersion);
        Move(RecoveryCaseStatus.Cancelled, now, RecoveryCaseStatus.Draft, RecoveryCaseStatus.Planning,
            RecoveryCaseStatus.AwaitingInputs, RecoveryCaseStatus.AwaitingApproval, RecoveryCaseStatus.RevisionRequested);
    }

    private void SetInputs(RecoveryInputs inputs, AssessmentSummary assessment)
    {
        Objective = inputs.Objective.Trim();
        PreferredRoutesJson = JsonSerializer.Serialize(inputs.PreferredRoutes);
        Currency = inputs.Currency;
        MaximumPickupCost = inputs.MaximumPickupCost;
        Deadline = inputs.Deadline?.ToUniversalTime();
        AssessmentId = assessment.AssessmentId;
        AssessmentVersion = assessment.AssessmentVersion;
        ItemRevision = assessment.ItemRevision;
    }

    private void RequireState(params RecoveryCaseStatus[] allowed)
    {
        if (!allowed.Contains(Status)) throw RecoveryException.Conflict("invalid_transition", $"This operation is not allowed while the case is {Status}.");
    }

    private void Move(RecoveryCaseStatus target, DateTimeOffset now, params RecoveryCaseStatus[] allowed)
    {
        RequireState(allowed);
        Status = target;
        Touch(now);
    }
}
