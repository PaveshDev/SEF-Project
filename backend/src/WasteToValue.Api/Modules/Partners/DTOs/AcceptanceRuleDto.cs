namespace WasteToValue.Api.Modules.Partners.DTOs;

public record AcceptanceRuleResponse(
    Guid Id,
    Guid PartnerId,
    Guid CategoryId,
    string RouteType,
    string MinimumCondition,
    string Restrictions,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreateAcceptanceRuleRequest(
    Guid PartnerId,
    Guid CategoryId,
    string RouteType,
    string MinimumCondition,
    string Restrictions
);

public record UpdateAcceptanceRuleRequest(
    string RouteType,
    string MinimumCondition,
    string Restrictions,
    bool IsActive
);
