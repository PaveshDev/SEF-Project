using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class RecoveryIdempotencyTests
{
    [Fact]
    public async Task Service_Participates_In_Command_Replay()
    {
        var f = new Fixture();
        var request = new CreateRecoveryCaseRequest(f.Assessments.Value.ItemId, f.Inputs);
        var first = await f.Planning.CreateAsync(request, "same-key", default);
        var replay = await f.Planning.CreateAsync(request, "same-key", default);

        Assert.Equal(first, replay);
        Assert.Single(f.Repo.Cases);
        Assert.Equal(1, f.Commands.Executions);
    }

    [Fact]
    public async Task Service_Rejects_Idempotency_Conflict_On_Different_Payload()
    {
        var f = new Fixture();
        var request = new CreateRecoveryCaseRequest(f.Assessments.Value.ItemId, f.Inputs);
        await f.Planning.CreateAsync(request, "same-key", default);

        await Assert.ThrowsAsync<RecoveryException>(async () =>
            await f.Planning.CreateAsync(request with { Inputs = f.Inputs with { Objective = "Changed" } }, "same-key", default));

        // The exception should have idempotency_conflict code
        var exception = await Record.ExceptionAsync(async () =>
            await f.Planning.CreateAsync(request with { Inputs = f.Inputs with { Objective = "Changed" } }, "same-key", default));

        Assert.Equal("idempotency_conflict", ((RecoveryException)exception).Code);
    }

    [Fact]
    public void Command_Generates_Stable_Distinct_Downstream_Operation_IDs()
    {
        var f = new Fixture();
        var request = new CreateRecoveryCaseRequest(f.Assessments.Value.ItemId, f.Inputs);
        var command = RecoveryCommand.Create(f.Actors.Actor, "test", "key", request);

        var match1 = command.ChildOperation("match");
        var match2 = command.ChildOperation("match");
        var pickup = command.ChildOperation("pickup");

        Assert.Equal(match1, match2);
        Assert.NotEqual(match1, pickup);
    }

    [Fact]
    public async Task Unavailable_Gateway_Does_Not_Cause_Mutation_Without_Durable_Receipts()
    {
        var f = new Fixture();
        var unavailable = new UnavailableRecoveryIntegrations();
        var executed = false;

        var request = new CreateRecoveryCaseRequest(f.Assessments.Value.ItemId, f.Inputs);
        var command = RecoveryCommand.Create(f.Actors.Actor, "test", "key", request);

        await Assert.ThrowsAsync<RecoveryException>(async () =>
            await unavailable.ExecuteAsync(command, _ => Task.CompletedTask,
                _ => { executed = true; return Task.FromResult(1); }, default));

        Assert.False(executed);
    }
}