namespace WasteToValue.Api.Modules.Collections.DTOs;

public sealed record ApprovalDecisionDto(
    Guid ProposalId,
    string Decision,
    string? Comment,
    Guid DecidedBy);
