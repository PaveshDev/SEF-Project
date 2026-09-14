using WasteToValue.Api.Modules.Recovery.DTOs;
using WasteToValue.Api.Modules.Recovery.Interfaces;
using WasteToValue.Api.Modules.Recovery.Validators;

namespace WasteToValue.Recovery.Agent;

public interface IRecoveryWorkflowApprovalAuthorizer
{
    Task AuthorizeAsync(RecoveryWorkflowState workflow, RecoveryApprovalDecision decision, CancellationToken ct);
}

// Resume consumes an already recorded owner decision. It never treats a client
// supplied decision as evidence that the proposal was actually approved.
public sealed class RecoveryWorkflowApprovalAuthorizer(IProposalDecisionService proposals) : IRecoveryWorkflowApprovalAuthorizer
{
    public async Task AuthorizeAsync(RecoveryWorkflowState workflow, RecoveryApprovalDecision decision, CancellationToken ct)
    {
        var proposal = await proposals.GetAsync(decision.ProposalId, ct); // Owned lookup with the current verified identity.
        var expected = decision.Decision switch
        {
            ProposalDecisionKind.Approved => RecoveryProposalStatus.Approved,
            ProposalDecisionKind.Rejected => RecoveryProposalStatus.Rejected,
            ProposalDecisionKind.RevisionRequested => RecoveryProposalStatus.RevisionRequested,
            _ => throw RecoveryException.Invalid("Unknown decision.")
        };
        if (proposal.CaseId != workflow.RecoveryCaseId || proposal.Revision != decision.ProposalRevision ||
            proposal.Id != workflow.ProposalId || proposal.Status != expected)
            throw RecoveryException.Conflict("approval_not_recorded", "The matching owner decision has not been recorded for this workflow.");
    }
}
