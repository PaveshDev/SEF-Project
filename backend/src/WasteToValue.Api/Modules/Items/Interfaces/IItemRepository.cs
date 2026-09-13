using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WasteToValue.Api.Modules.Items.Entities;

namespace WasteToValue.Api.Modules.Items.Interfaces;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Item?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task AddAsync(Item item, CancellationToken cancellationToken = default);
    void Update(Item item);
    void Delete(Item item);
    
    Task<ItemPhoto?> GetPhotoByIdAsync(Guid photoId, CancellationToken cancellationToken = default);
    Task AddPhotoAsync(ItemPhoto photo, CancellationToken cancellationToken = default);
    void DeletePhoto(ItemPhoto photo);
    
    Task<IEnumerable<ItemAssessment>> GetAssessmentsByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<ItemAssessment?> GetAssessmentByIdAsync(Guid assessmentId, CancellationToken cancellationToken = default);
    Task AddAssessmentAsync(ItemAssessment assessment, CancellationToken cancellationToken = default);
    
    Task<AssessmentClarification?> GetClarificationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddClarificationAsync(AssessmentClarification clarification, CancellationToken cancellationToken = default);
    
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
