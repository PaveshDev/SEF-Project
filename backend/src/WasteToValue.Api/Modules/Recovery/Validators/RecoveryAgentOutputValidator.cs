using WasteToValue.Api.Modules.Recovery.DTOs.Agent;

namespace WasteToValue.Api.Modules.Recovery.Validators;

public sealed class RecoveryAgentOutputValidator
{
    public void Validate(RecoveryAgentInput input, RecoveryAgentOutput output)
    {
        if (output.ContractVersion != 1 || input.ContractVersion != 1 || output.RunId != input.RunId ||
            output.CaseId != input.CaseId || output.CaseRevision != input.CaseRevision)
            throw RecoveryException.Conflict("stale_agent_output", "Agent output does not match the current run and case revision.");
        if (output.Candidates is null || output.Candidates.Count > 5 ||
            output.Candidates.Any(c => c is null) ||
            output.Candidates.Select(c => c.Route).Distinct().Count() != output.Candidates.Count)
            throw RecoveryException.Invalid("Agent candidates must be a bounded set of distinct routes.");
        var available = input.VerifiedValueReferences.Select(r => r.ReferenceId).ToHashSet();
        foreach (var candidate in output.Candidates)
        {
            RecoveryRequestValidator.Defined(candidate.Route);
            if (!input.AllowedRoutes.Contains(candidate.Route) || candidate.ReferenceIds is null ||
                candidate.ReferenceIds.Count == 0 || candidate.ReferenceIds.Count > 20 ||
                candidate.ReferenceIds.Any(id => !available.Contains(id)) ||
                candidate.ReferenceIds.Distinct().Count() != candidate.ReferenceIds.Count)
                throw RecoveryException.Invalid("Agent recommendations must use allowed routes and supplied evidence.");
            RecoveryRequestValidator.Text(candidate.Rationale, "Rationale", 2000);
            ValidateStrings(candidate.NonFinancialBenefits);
        }
        if (output.PreferredRoute is { } preferred && !output.Candidates.Any(c => c.Route == preferred))
            throw RecoveryException.Invalid("The preferred route must identify a returned candidate.");
        if (!output.AwaitingInputs && (output.Candidates.Count == 0 || output.PreferredRoute is null || input.MissingInputs.Count > 0))
            throw RecoveryException.Invalid("Missing inputs cannot be presented as a ready recommendation.");
        ValidateStrings(output.Unknowns); ValidateStrings(output.Warnings); ValidateStrings(output.ClarificationRequests);
    }

    private static void ValidateStrings(IReadOnlyList<string>? values)
    {
        if (values is null || values.Count > 20) throw RecoveryException.Invalid("Agent text lists must contain at most 20 entries.");
        foreach (var value in values) RecoveryRequestValidator.Text(value, "Agent text", 1000);
    }
}
