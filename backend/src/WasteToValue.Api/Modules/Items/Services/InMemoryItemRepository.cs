using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WasteToValue.Api.Modules.Items.Entities;
using WasteToValue.Api.Modules.Items.Interfaces;

namespace WasteToValue.Api.Modules.Items.Services;

public class InMemoryItemRepository : IItemRepository
{
    private readonly List<Item> _items = new();
    private readonly List<ItemPhoto> _photos = new();
    private readonly List<ItemAssessment> _assessments = new();
    private readonly List<AssessmentClarification> _clarifications = new();

    public Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    }

    public Task<Item?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            item.Photos = _photos.Where(p => p.ItemId == id).ToList();
            item.Assessments = _assessments.Where(a => a.ItemId == id).ToList();
            item.Clarifications = _clarifications.Where(c => c.ItemId == id).ToList();
        }
        return Task.FromResult(item);
    }

    public Task<IEnumerable<Item>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.Where(x => x.OwnerId == ownerId).OrderByDescending(x => x.CreatedAt).AsEnumerable());
    }

    public Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        _items.Add(item);
        return Task.CompletedTask;
    }

    public void Update(Item item)
    {
        var index = _items.FindIndex(x => x.Id == item.Id);
        if (index >= 0) _items[index] = item;
    }

    public void Delete(Item item)
    {
        _items.Remove(item);
    }

    public Task<ItemPhoto?> GetPhotoByIdAsync(Guid photoId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_photos.FirstOrDefault(x => x.Id == photoId));
    }

    public Task AddPhotoAsync(ItemPhoto photo, CancellationToken cancellationToken = default)
    {
        _photos.Add(photo);
        return Task.CompletedTask;
    }

    public void DeletePhoto(ItemPhoto photo)
    {
        _photos.Remove(photo);
    }

    public Task<IEnumerable<ItemAssessment>> GetAssessmentsByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_assessments.Where(x => x.ItemId == itemId).OrderByDescending(x => x.Version).AsEnumerable());
    }

    public Task<ItemAssessment?> GetAssessmentByIdAsync(Guid assessmentId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_assessments.FirstOrDefault(x => x.Id == assessmentId));
    }

    public Task AddAssessmentAsync(ItemAssessment assessment, CancellationToken cancellationToken = default)
    {
        _assessments.Add(assessment);
        return Task.CompletedTask;
    }

    public Task<AssessmentClarification?> GetClarificationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_clarifications.FirstOrDefault(x => x.Id == id));
    }

    public Task AddClarificationAsync(AssessmentClarification clarification, CancellationToken cancellationToken = default)
    {
        _clarifications.Add(clarification);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
