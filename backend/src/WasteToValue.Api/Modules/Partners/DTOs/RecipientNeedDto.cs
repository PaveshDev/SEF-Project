namespace WasteToValue.Api.Modules.Partners.DTOs;

public record RecipientNeedResponse(
    Guid Id,
    Guid PartnerId,
    Guid CategoryId,
    string Description,
    int QuantityRequired,
    int QuantityFulfilled,
    DateTimeOffset? Deadline,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreateRecipientNeedRequest(
    Guid PartnerId,
    Guid CategoryId,
    string Description,
    int QuantityRequired,
    DateTimeOffset? Deadline
);

public record UpdateRecipientNeedRequest(
    string Description,
    int QuantityRequired,
    DateTimeOffset? Deadline,
    string Status
);
