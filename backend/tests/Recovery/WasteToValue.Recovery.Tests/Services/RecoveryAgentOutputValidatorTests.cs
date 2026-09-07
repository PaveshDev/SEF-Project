using System.Text.Json;
using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.DTOs.Agent;
using WasteToValue.Api.Modules.Recovery.Services;
using WasteToValue.Api.Modules.Recovery.Validators;
using Xunit;

namespace WasteToValue.Recovery.Tests.Services;

public class RecoveryAgentOutputValidatorTests
{
    [Fact]
    public void Validator_Accepts_Valid_Output()
    {
        var f = new Fixture();
        var referenceId = Guid.NewGuid();

        var input = new RecoveryAgentInput(1, Guid.NewGuid(), Guid.NewGuid(), 1, "Reuse", new[] { RecoveryRoute.Reuse },
            null, "LKR", null, new(Guid.NewGuid(), 1, 1, ConditionGrade.Good, FunctionalStatus.Working, Array.Empty<string>()),
            new[] { new ValueEvidence(referenceId, 1, f.Clock.Now, 10, 20, "LKR", "Reference") }, Array.Empty<string>());

        var output = new RecoveryAgentOutput(1, input.RunId, input.CaseId, 1,
            new[] { new RecoveryAgentCandidate(RecoveryRoute.Reuse, new[] { referenceId }, "Based on supplied evidence.", Array.Empty<string>()) },
            RecoveryRoute.Reuse, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), false);

        var validator = new RecoveryAgentOutputValidator();
        validator.Validate(input, output); // Should not throw
    }

    [Fact]
    public void Validator_Rejects_Stale_Agent_Output()
    {
        var f = new Fixture();
        var referenceId = Guid.NewGuid();

        var input = new RecoveryAgentInput(1, Guid.NewGuid(), Guid.NewGuid(), 1, "Reuse", new[] { RecoveryRoute.Reuse },
            null, "LKR", null, new(Guid.NewGuid(), 1, 1, ConditionGrade.Good, FunctionalStatus.Working, Array.Empty<string>()),
            new[] { new ValueEvidence(referenceId, 1, f.Clock.Now, 10, 20, "LKR", "Reference") }, Array.Empty<string>());

        var output = new RecoveryAgentOutput(1, input.RunId, input.CaseId, 1,
            new[] { new RecoveryAgentCandidate(RecoveryRoute.Reuse, new[] { referenceId }, "Based on supplied evidence.", Array.Empty<string>()) },
            RecoveryRoute.Reuse, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), false);

        var validator = new RecoveryAgentOutputValidator();

        var exception = Assert.Throws<RecoveryException>(() =>
            validator.Validate(input, output with { CaseRevision = 2 }));

        Assert.Equal("stale_agent_output", exception.Code);
    }

    [Fact]
    public void Validator_Rejects_Invalid_Input()
    {
        var f = new Fixture();
        var referenceId = Guid.NewGuid();

        var input = new RecoveryAgentInput(1, Guid.NewGuid(), Guid.NewGuid(), 1, "Reuse", new[] { RecoveryRoute.Reuse },
            null, "LKR", null, new(Guid.NewGuid(), 1, 1, ConditionGrade.Good, FunctionalStatus.Working, Array.Empty<string>()),
            new[] { new ValueEvidence(referenceId, 1, f.Clock.Now, 10, 20, "LKR", "Reference") }, Array.Empty<string>());

        var output = new RecoveryAgentOutput(1, input.RunId, input.CaseId, 1,
            new[] { new RecoveryAgentCandidate(RecoveryRoute.Reuse, new[] { referenceId }, "Based on supplied evidence.", Array.Empty<string>()) },
            RecoveryRoute.Reuse, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), false);

        var validator = new RecoveryAgentOutputValidator();

        var exception = Assert.Throws<RecoveryException>(() =>
            validator.Validate(input with { MissingInputs = new[] { "Assessment" } }, output));

        Assert.Equal("invalid_input", exception.Code);
    }

    [Fact]
    public void Validator_Rejects_Invented_Evidence()
    {
        var f = new Fixture();
        var referenceId = Guid.NewGuid();

        var input = new RecoveryAgentInput(1, Guid.NewGuid(), Guid.NewGuid(), 1, "Reuse", new[] { RecoveryRoute.Reuse },
            null, "LKR", null, new(Guid.NewGuid(), 1, 1, ConditionGrade.Good, FunctionalStatus.Working, Array.Empty<string>()),
            new[] { new ValueEvidence(referenceId, 1, f.Clock.Now, 10, 20, "LKR", "Reference") }, Array.Empty<string>());

        var output = new RecoveryAgentOutput(1, input.RunId, input.CaseId, 1,
            new[] { new RecoveryAgentCandidate(RecoveryRoute.Reuse, new[] { referenceId }, "Based on supplied evidence.", Array.Empty<string>()) },
            RecoveryRoute.Reuse, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), false);

        var validator = new RecoveryAgentOutputValidator();

        var exception = Assert.Throws<RecoveryException>(() =>
            validator.Validate(input, output with { Candidates = new[] {
                new RecoveryAgentCandidate(RecoveryRoute.Reuse, new[] { Guid.NewGuid() }, "Invented evidence", Array.Empty<string>()) } }));

        Assert.Equal("invalid_input", exception.Code);
    }

    [Fact]
    public void Validator_Rejects_Approval_Field_In_Agent_Output()
    {
        var f = new Fixture();
        var referenceId = Guid.NewGuid();

        var input = new RecoveryAgentInput(1, Guid.NewGuid(), Guid.NewGuid(), 1, "Reuse", new[] { RecoveryRoute.Reuse },
            null, "LKR", null, new(Guid.NewGuid(), 1, 1, ConditionGrade.Good, FunctionalStatus.Working, Array.Empty<string>()),
            new[] { new ValueEvidence(referenceId, 1, f.Clock.Now, 10, 20, "LKR", "Reference") }, Array.Empty<string>());

        var output = new RecoveryAgentOutput(1, input.RunId, input.CaseId, 1,
            new[] { new RecoveryAgentCandidate(RecoveryRoute.Reuse, new[] { referenceId }, "Based on supplied evidence.", Array.Empty<string>()) },
            RecoveryRoute.Reuse, Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), false);

        var injected = JsonSerializer.Serialize(output).TrimEnd('}') + ",\"Approved\":true}";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<RecoveryAgentOutput>(injected));
    }
}