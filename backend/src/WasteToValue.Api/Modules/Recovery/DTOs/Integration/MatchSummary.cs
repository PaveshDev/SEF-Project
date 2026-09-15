namespace WasteToValue.Api.Modules.Recovery.DTOs.Integration;

public sealed record MatchSummary(Guid MatchId, Guid RecoveryOptionId, Guid PartnerId,
    Guid? RecipientNeedId, int Version, MatchEligibility Eligibility, PartnerResponse Response,
    string FreshnessToken, DateTimeOffset CheckedAt, IReadOnlyList<string> Reasons);
