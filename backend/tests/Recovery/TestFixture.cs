using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
using WasteToValue.Api.Modules.Recovery.Entities;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;

// Test doubles only. No fake upstream services or in-memory receipts are registered in the application.
internal sealed class TestRepository : IRecoveryRepository
{
    public List<RecoveryCase> Cases = new();
    public List<RecoveryOption> Options = new();
    public List<RecoveryProposal> Proposals = new();
    public List<ProposalDecision> Decisions = new();
    public List<ValueReference> References = new();
    public Task<RecoveryCase?> FindCaseAsync(Guid id, CancellationToken ct) => Task.FromResult(Cases.SingleOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<RecoveryCase>> ListCasesAsync(Guid ownerId, CancellationToken ct) => Task.FromResult<IReadOnlyList<RecoveryCase>>(Cases.Where(x => x.OwnerId == ownerId).ToArray());
    public Task<bool> HasActiveCaseAsync(Guid itemId, Guid? exceptId, CancellationToken ct) => Task.FromResult(Cases.Any(x => x.ItemId == itemId && x.Id != exceptId && x.IsActive));
    public Task<IReadOnlyList<RecoveryOption>> ListOptionsAsync(Guid caseId, CancellationToken ct) => Task.FromResult<IReadOnlyList<RecoveryOption>>(Options.Where(x => x.RecoveryCaseId == caseId).ToArray());
    public Task<RecoveryOption?> FindOptionAsync(Guid id, CancellationToken ct) => Task.FromResult(Options.SingleOrDefault(x => x.Id == id));
    public Task<RecoveryProposal?> FindProposalAsync(Guid id, CancellationToken ct) => Task.FromResult(Proposals.SingleOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<RecoveryProposal>> ListProposalsAsync(Guid caseId, CancellationToken ct) => Task.FromResult<IReadOnlyList<RecoveryProposal>>(Proposals.Where(x => x.RecoveryCaseId == caseId).ToArray());
    public Task<IReadOnlyList<ValueReference>> ListReferencesAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<ValueReference>>(References.ToArray());
    public Task<ValueReference?> FindReferenceAsync(Guid id, CancellationToken ct) => Task.FromResult(References.SingleOrDefault(x => x.Id == id));
    public void Add(RecoveryCase value) { Check.AssignId(value); Cases.Add(value); }
    public void Add(RecoveryOption value) { Check.AssignId(value); Options.Add(value); }
    public void Add(RecoveryProposal value) { Check.AssignId(value); Proposals.Add(value); }
    public void Add(ProposalDecision value) { Check.AssignId(value); Decisions.Add(value); }
    public void Add(ValueReference value) { Check.AssignId(value); References.Add(value); }
    public Task SaveAsync(CancellationToken ct) { ct.ThrowIfCancellationRequested(); return Task.CompletedTask; }
}
internal sealed class TestActor : IRecoveryActorAccessor
{
    public RecoveryActor Actor = new(Guid.NewGuid(), true, true);
    public Task<RecoveryActor> GetAsync(CancellationToken ct) { ct.ThrowIfCancellationRequested(); return Task.FromResult(Actor); }
}
internal sealed class TestClock : TimeProvider
{
    public DateTimeOffset Now = new(2026, 9, 7, 10, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}
internal sealed class TestAssessment : IAssessmentGateway
{
    public required AssessmentSummary Value;
    public CancellationToken LastToken;
    public Task<GatewayResult<AssessmentSummary>> GetCurrentAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested(); LastToken = ct;
        return Task.FromResult(GatewayResult<AssessmentSummary>.Success(Value));
    }
}
internal sealed class TestCommands : IRecoveryCommandExecutor
{
    private readonly Dictionary<(Guid, string, string), (string Hash, object? Value)> receipts = new();
    public int Executions;
    public async Task<T> ExecuteAsync<T>(RecoveryCommand command, Func<CancellationToken, Task> authorize,
        Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await authorize(ct); // Even replay must reauthorize.
        var key = (command.ActorId, command.Operation, command.Key);
        if (receipts.TryGetValue(key, out var old))
        {
            if (old.Hash != command.RequestHash) throw RecoveryException.Conflict("idempotency_conflict", "Different payload.");
            return (T)old.Value!;
        }
        var value = await action(ct); Executions++;
        receipts[key] = (command.RequestHash, value);
        return value;
    }
}
internal sealed class Fixture
{
    public TestRepository Repo = new();
    public TestActor Actors = new();
    public TestClock Clock = new();
    public TestCommands Commands = new();
    public TestAssessment Assessments;
    public RecoveryPlanningService Planning;
    public ProposalDecisionService Proposals;
    public RecoveryInputs Inputs => new("Keep the item useful", new[] { RecoveryRoute.Reuse }, "LKR", null, Clock.Now.AddDays(2));
    public Fixture()
    {
        Assessments = new() { Value = new(Guid.NewGuid(), Guid.NewGuid(), Actors.Actor.UserId, Guid.NewGuid(),
            1, 1, true, AssessmentStatus.Confirmed, ConditionGrade.Good, FunctionalStatus.Working, Clock.Now,
            "General service area", Array.Empty<string>()) };
        var access = new RecoveryAccess(Actors, Repo);
        var missing = new UnavailableRecoveryIntegrations();
        Planning = new(access, Repo, Commands, Assessments, missing, missing, new ValueEstimationService(), Clock);
        Proposals = new(access, Repo, Commands, Assessments, missing, missing, Clock);
    }
    public async Task<(RecoveryCaseResponse Case, RecoveryOptionResponse Option)> PlanReuseAsync()
    {
        var request = new CreateValueReferenceRequest(Assessments.Value.CategoryId!.Value,
            ConditionGrade.Good, RecoveryRoute.Reuse, 100, 200, "LKR", "Verified test evidence", null, Clock.Now);
        var reference = ValueReference.Create(request, Actors.Actor.UserId, Clock.Now);
        reference.Verify(1, Clock.Now); Repo.Add(reference);
        var value = await Planning.CreateAsync(new(Assessments.Value.ItemId, Inputs), "create", default);
        var planned = await Planning.PlanAsync(value.Id, new(value.Version), "plan", default);
        return (planned.Case, planned.Options.Single());
    }
    public async Task<RecoveryProposalResponse> SubmitAsync()
    {
        var (value, option) = await PlanReuseAsync();
        return await Proposals.SubmitAsync(value.Id, new(value.Version, option.Id, option.Version,
            null, null, Clock.Now.AddHours(2), "Reuse with verified evidence."), "submit", default);
    }
}
