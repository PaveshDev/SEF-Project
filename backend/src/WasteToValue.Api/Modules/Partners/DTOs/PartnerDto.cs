namespace WasteToValue.Api.Modules.Partners.DTOs;

public record PartnerResponse(
    Guid Id,
    string Name,
    string PartnerType,
    string VerificationStatus,
    bool IsActive,
    string ServiceArea,
    string? ContactEmail,
    string? ContactPhone,
    int Capacity,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record CreatePartnerRequest(
    string Name,
    string PartnerType,
    string ServiceArea,
    string? ContactEmail,
    string? ContactPhone,
    int Capacity
);

public record UpdatePartnerRequest(
    string Name,
    string PartnerType,
    string ServiceArea,
    string? ContactEmail,
    string? ContactPhone,
    int Capacity
);
