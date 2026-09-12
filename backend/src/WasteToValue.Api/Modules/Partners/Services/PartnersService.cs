using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Partners.DTOs;
using WasteToValue.Api.Modules.Partners.Entities;
using WasteToValue.Api.Modules.Partners.Interfaces;

namespace WasteToValue.Api.Modules.Partners.Services;

public class PartnersService(AppDbContext dbContext) : IPartnersService
{
    public async Task<IEnumerable<PartnerResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var partners = await dbContext.Set<Partner>().ToListAsync(cancellationToken);
        return partners.Select(MapToResponse);
    }

    public async Task<PartnerResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var partner = await dbContext.Set<Partner>().FindAsync(new object[] { id }, cancellationToken);
        return partner != null ? MapToResponse(partner) : null;
    }

    public async Task<PartnerResponse> CreateAsync(CreatePartnerRequest request, CancellationToken cancellationToken = default)
    {
        var partner = new Partner
        {
            Name = request.Name,
            PartnerType = Enum.Parse<PartnerType>(request.PartnerType, true),
            ServiceArea = request.ServiceArea,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Capacity = request.Capacity,
            VerificationStatus = VerificationStatus.Pending,
            IsActive = true
        };

        dbContext.Set<Partner>().Add(partner);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(partner);
    }

    public async Task<PartnerResponse?> UpdateAsync(Guid id, UpdatePartnerRequest request, CancellationToken cancellationToken = default)
    {
        var partner = await dbContext.Set<Partner>().FindAsync(new object[] { id }, cancellationToken);
        if (partner == null) return null;

        partner.Name = request.Name;
        partner.PartnerType = Enum.Parse<PartnerType>(request.PartnerType, true);
        partner.ServiceArea = request.ServiceArea;
        partner.ContactEmail = request.ContactEmail;
        partner.ContactPhone = request.ContactPhone;
        partner.Capacity = request.Capacity;
        partner.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(partner);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var partner = await dbContext.Set<Partner>().FindAsync(new object[] { id }, cancellationToken);
        if (partner == null) return false;

        dbContext.Set<Partner>().Remove(partner);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PartnerResponse MapToResponse(Partner p) =>
        new(p.Id, p.Name, p.PartnerType.ToString(), p.VerificationStatus.ToString(), p.IsActive, p.ServiceArea, p.ContactEmail, p.ContactPhone, p.Capacity, p.CreatedAt, p.UpdatedAt);
}
