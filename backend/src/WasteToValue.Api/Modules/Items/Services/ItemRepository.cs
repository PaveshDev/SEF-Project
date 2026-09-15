using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Items.Entities;
using WasteToValue.Api.Modules.Items.Interfaces;

namespace WasteToValue.Api.Modules.Items.Services;

public class ItemRepository(AppDbContext context) : IItemRepository
{
    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Set<Item>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Item?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Set<Item>()
            .Include(x => x.Photos)
            .Include(x => x.ConditionAnswers)
            .Include(x => x.Assessments)
            .Include(x => x.Clarifications)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await context.Set<Item>()
            .Where(x => x.OwnerId == ownerId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        await context.Set<Item>().AddAsync(item, cancellationToken);
    }

    public void Update(Item item)
    {
        context.Set<Item>().Update(item);
    }

    public void Delete(Item item)
    {
        context.Set<Item>().Remove(item);
    }

    public async Task<ItemPhoto?> GetPhotoByIdAsync(Guid photoId, CancellationToken cancellationToken = default)
    {
        return await context.Set<ItemPhoto>()
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == photoId, cancellationToken);
    }

    public async Task AddPhotoAsync(ItemPhoto photo, CancellationToken cancellationToken = default)
    {
        await context.Set<ItemPhoto>().AddAsync(photo, cancellationToken);
    }

    public void DeletePhoto(ItemPhoto photo)
    {
        context.Set<ItemPhoto>().Remove(photo);
    }

    public async Task<IEnumerable<ItemAssessment>> GetAssessmentsByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await context.Set<ItemAssessment>()
            .Include(x => x.Evidences)
            .Include(x => x.Clarifications)
            .Where(x => x.ItemId == itemId)
            .OrderByDescending(x => x.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<ItemAssessment?> GetAssessmentByIdAsync(Guid assessmentId, CancellationToken cancellationToken = default)
    {
        return await context.Set<ItemAssessment>()
            .Include(x => x.Item)
            .Include(x => x.Evidences)
            .Include(x => x.Clarifications)
            .FirstOrDefaultAsync(x => x.Id == assessmentId, cancellationToken);
    }

    public async Task AddAssessmentAsync(ItemAssessment assessment, CancellationToken cancellationToken = default)
    {
        await context.Set<ItemAssessment>().AddAsync(assessment, cancellationToken);
    }

    public async Task<AssessmentClarification?> GetClarificationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Set<AssessmentClarification>()
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddClarificationAsync(AssessmentClarification clarification, CancellationToken cancellationToken = default)
    {
        await context.Set<AssessmentClarification>().AddAsync(clarification, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
