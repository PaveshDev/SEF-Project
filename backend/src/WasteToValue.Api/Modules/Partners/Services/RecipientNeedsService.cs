using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Partners.DTOs;
using WasteToValue.Api.Modules.Partners.Entities;
using WasteToValue.Api.Modules.Partners.Interfaces;

namespace WasteToValue.Api.Modules.Partners.Services;

public class RecipientNeedsService(AppDbContext dbContext) : IRecipientNeedsService
{
    public async Task<IEnumerable<RecipientNeedResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var needs = await dbContext.Set<RecipientNeed>().ToListAsync(cancellationToken);
        return needs.Select(MapToResponse);
    }

    public async Task<RecipientNeedResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var need = await dbContext.Set<RecipientNeed>().FindAsync(new object[] { id }, cancellationToken);
        return need != null ? MapToResponse(need) : null;
    }

    public async Task<RecipientNeedResponse> CreateAsync(CreateRecipientNeedRequest request, CancellationToken cancellationToken = default)
    {
        var need = new RecipientNeed
        {
            PartnerId = request.PartnerId,
            CategoryId = request.CategoryId,
            Description = request.Description,
            QuantityRequired = request.QuantityRequired,
            Deadline = request.Deadline,
            Status = NeedStatus.Open
        };

        dbContext.Set<RecipientNeed>().Add(need);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(need);
    }

    public async Task<RecipientNeedResponse?> UpdateAsync(Guid id, UpdateRecipientNeedRequest request, CancellationToken cancellationToken = default)
    {
        var need = await dbContext.Set<RecipientNeed>().FindAsync(new object[] { id }, cancellationToken);
        if (need == null) return null;

        need.Description = request.Description;
        need.QuantityRequired = request.QuantityRequired;
        need.Deadline = request.Deadline;
        need.Status = Enum.Parse<NeedStatus>(request.Status, true);
        need.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(need);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var need = await dbContext.Set<RecipientNeed>().FindAsync(new object[] { id }, cancellationToken);
        if (need == null) return false;

        dbContext.Set<RecipientNeed>().Remove(need);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static RecipientNeedResponse MapToResponse(RecipientNeed n) =>
        new(n.Id, n.PartnerId, n.CategoryId, n.Description, n.QuantityRequired, n.QuantityFulfilled, n.Deadline, n.Status.ToString(), n.CreatedAt, n.UpdatedAt);
}
