using WasteToValue.Api.Modules.Recovery.DTOs;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public interface IProposalDecisionService
{
    Task<RecoveryProposalResponse> SubmitAsync(Guid caseId, SubmitProposalRequest request, string key, CancellationToken ct);
    Task<RecoveryProposalResponse> GetAsync(Guid proposalId, CancellationToken ct);
    Task<RecoveryProposalResponse> DecideAsync(Guid proposalId, ProposalDecisionRequest request, string key, CancellationToken ct);
    Task<RecoveryProposalResponse> RefreshAsync(Guid proposalId, string key, CancellationToken ct);
}
