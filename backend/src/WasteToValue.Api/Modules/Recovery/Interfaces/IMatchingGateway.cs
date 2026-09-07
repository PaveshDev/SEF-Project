using WasteToValue.Api.Modules.Recovery.DTOs.Integration;
namespace WasteToValue.Api.Modules.Recovery.Interfaces;

public interface IMatchingGateway
{
    Task<GatewayResult<IReadOnlyList<MatchSummary>>> FindMatchesAsync(MatchRequest request, CancellationToken cancellationToken);
    Task<GatewayResult<MatchSummary>> RevalidateAsync(Guid matchId, int expectedVersion,
        string expectedFreshnessToken, CancellationToken cancellationToken);
}
