using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Partners.DTOs;
using WasteToValue.Api.Modules.Partners.Entities;
using WasteToValue.Api.Modules.Partners.Interfaces;

namespace WasteToValue.Api.Modules.Partners.Services;

public class AcceptanceRulesService(AppDbContext dbContext) : IAcceptanceRulesService
{
    public async Task<IEnumerable<AcceptanceRuleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await dbContext.Set<AcceptanceRule>().ToListAsync(cancellationToken);
        return rules.Select(MapToResponse);
    }

    public async Task<AcceptanceRuleResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await dbContext.Set<AcceptanceRule>().FindAsync(new object[] { id }, cancellationToken);
        return rule != null ? MapToResponse(rule) : null;
    }

    public async Task<AcceptanceRuleResponse> CreateAsync(CreateAcceptanceRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = new AcceptanceRule
        {
            PartnerId = request.PartnerId,
            CategoryId = request.CategoryId,
            RouteType = Enum.Parse<RouteType>(request.RouteType, true),
            MinimumCondition = Enum.Parse<MinimumCondition>(request.MinimumCondition, true),
            Restrictions = request.Restrictions,
            IsActive = true
        };

        dbContext.Set<AcceptanceRule>().Add(rule);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(rule);
    }

    public async Task<AcceptanceRuleResponse?> UpdateAsync(Guid id, UpdateAcceptanceRuleRequest request, CancellationToken cancellationToken = default)
    {
        var rule = await dbContext.Set<AcceptanceRule>().FindAsync(new object[] { id }, cancellationToken);
        if (rule == null) return null;

        rule.RouteType = Enum.Parse<RouteType>(request.RouteType, true);
        rule.MinimumCondition = Enum.Parse<MinimumCondition>(request.MinimumCondition, true);
        rule.Restrictions = request.Restrictions;
        rule.IsActive = request.IsActive;
        rule.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return MapToResponse(rule);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await dbContext.Set<AcceptanceRule>().FindAsync(new object[] { id }, cancellationToken);
        if (rule == null) return false;

        dbContext.Set<AcceptanceRule>().Remove(rule);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static AcceptanceRuleResponse MapToResponse(AcceptanceRule r) =>
        new(r.Id, r.PartnerId, r.CategoryId, r.RouteType.ToString(), r.MinimumCondition.ToString(), r.Restrictions, r.IsActive, r.CreatedAt, r.UpdatedAt);
}
