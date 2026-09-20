using LoopWorth.Application.Common.Interfaces;
using LoopWorth.Application.DTOs;
using LoopWorth.Domain.Entities;
using LoopWorth.Domain.Enums;
using LoopWorth.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoopWorth.Api.Controllers;

[ApiController]
[Route("api/items")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IItemAssessmentAgent _agent;
    private readonly IFileStorageService _fileStorage;

    public ItemsController(AppDbContext context, IItemAssessmentAgent agent, IFileStorageService fileStorage)
    {
        _context = context;
        _agent = agent;
        _fileStorage = fileStorage;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    [HttpPost]
    public async Task<IActionResult> Create(CreateItemDto dto)
    {
        var item = new Item
        {
            CustomerId = GetUserId(),
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            Brand = dto.Brand,
            Model = dto.Model,
            ConditionDescription = dto.ConditionDescription,
            Status = ItemStatus.Draft
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, MapToDto(item));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var items = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Images)
            .Where(i => i.CustomerId == userId)
            .ToListAsync();
        return Ok(items.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Images)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return NotFound();
        if (item.CustomerId != userId && !User.IsInRole("Admin")) return Forbid();

        return Ok(MapToDto(item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateItemDto dto)
    {
        var userId = GetUserId();
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();
        if (item.CustomerId != userId) return Forbid();
        if (item.Status != ItemStatus.Draft) return BadRequest("Only draft items can be updated.");

        item.Name = dto.Name;
        item.CategoryId = dto.CategoryId;
        item.Brand = dto.Brand;
        item.Model = dto.Model;
        item.ConditionDescription = dto.ConditionDescription;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var item = await _context.Items.Include(i => i.Images).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        if (item.CustomerId != userId) return Forbid();
        if (item.Status != ItemStatus.Draft) return BadRequest("Only draft items can be deleted.");

        foreach (var img in item.Images)
        {
            await _fileStorage.DeleteFileAsync(img.ImageUrl);
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/photos/upload")]
    public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file)
    {
        var userId = GetUserId();
        var item = await _context.Items.Include(i => i.Images).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        if (item.CustomerId != userId) return Forbid();
        if (item.Status != ItemStatus.Draft) return BadRequest("Cannot add photos unless in Draft status.");

        using var stream = file.OpenReadStream();
        var url = await _fileStorage.SaveFileAsync(stream, file.FileName, "items");
        var image = new ItemImage
        {
            ItemId = item.Id,
            ImageUrl = url,
            SortOrder = item.Images.Count,
            IsPrimary = item.Images.Count == 0
        };
        _context.ItemImages.Add(image);
        await _context.SaveChangesAsync();

        return Ok(new ItemImageDto { Id = image.Id, ImageUrl = image.ImageUrl, SortOrder = image.SortOrder, IsPrimary = image.IsPrimary });
    }

    [HttpDelete("{id}/photos/{photoId}")]
    public async Task<IActionResult> DeletePhoto(Guid id, Guid photoId)
    {
        var userId = GetUserId();
        var item = await _context.Items.Include(i => i.Images).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        if (item.CustomerId != userId) return Forbid();

        var photo = item.Images.FirstOrDefault(p => p.Id == photoId);
        if (photo == null) return NotFound();

        await _fileStorage.DeleteFileAsync(photo.ImageUrl);
        _context.ItemImages.Remove(photo);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id)
    {
        var userId = GetUserId();
        var item = await _context.Items.Include(i => i.Images).FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        if (item.CustomerId != userId) return Forbid();
        if (item.Status != ItemStatus.Draft) return BadRequest("Only drafts can be submitted.");

        if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.ConditionDescription) || !item.Images.Any())
            return BadRequest("Name, ConditionDescription, and at least one photo are required.");

        item.Status = ItemStatus.Submitted;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/assess")]
    public async Task<IActionResult> Assess(Guid id)
    {
        var userId = GetUserId();
        var item = await _context.Items
            .Include(i => i.Category)
            .Include(i => i.Images)
            .FirstOrDefaultAsync(i => i.Id == id);
            
        if (item == null) return NotFound();
        if (item.CustomerId != userId && !User.IsInRole("Admin")) return Forbid();
        if (item.Status != ItemStatus.Submitted && item.Status != ItemStatus.AssessmentPending) 
            return BadRequest("Item must be submitted before assessment.");

        item.Status = ItemStatus.AssessmentPending;
        await _context.SaveChangesAsync();

        var workflow = new AgentWorkflow
        {
            CustomerId = userId,
            ItemId = item.Id,
            Objective = $"Complete recovery workflow for Item {item.Id}",
            CurrentStage = "Assessment",
            Status = "Running"
        };
        _context.AgentWorkflows.Add(workflow);

        var step = new AgentWorkflowStep
        {
            WorkflowId = workflow.Id,
            AgentName = "ItemAssessmentAgent",
            StepName = "AssessItem",
            ExecutionStatus = "Running",
            StartedAt = DateTime.UtcNow
        };
        _context.AgentWorkflowSteps.Add(step);
        await _context.SaveChangesAsync();

        try
        {
            var result = await _agent.AssessItemAsync(item);
            
            var assessment = new ItemAssessment
            {
                ItemId = item.Id,
                ConditionLevel = result.ConditionLevel,
                RecommendedRoute = result.RecommendedRoute,
                AlternativeRoute = result.AlternativeRoute,
                ConfidenceLevel = result.ConfidenceLevel,
                Explanation = result.Explanation
            };
            _context.ItemAssessments.Add(assessment);

            item.Status = ItemStatus.Assessed;
            
            step.ExecutionStatus = "Completed";
            step.OutputSummary = $"Recommended: {result.RecommendedRoute}";
            step.CompletedAt = DateTime.UtcNow;
            
            workflow.Status = "Completed";
            workflow.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            step.ExecutionStatus = "Failed";
            step.ErrorMessage = ex.Message;
            step.CompletedAt = DateTime.UtcNow;
            workflow.Status = "Failed";
            workflow.CompletedAt = DateTime.UtcNow;
            item.Status = ItemStatus.Submitted; // Revert status so they can retry
            await _context.SaveChangesAsync();
            return StatusCode(500, new { error = "Assessment could not be completed. Please retry." });
        }
    }

    [HttpGet("{id}/assessment")]
    public async Task<IActionResult> GetAssessment(Guid id)
    {
        var userId = GetUserId();
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        if (item.CustomerId != userId && !User.IsInRole("Admin")) return Forbid();

        var assessment = await _context.ItemAssessments
            .Where(a => a.ItemId == id)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync();
        if (assessment == null) return NotFound();
        return Ok(assessment);
    }

    [HttpPost("{id}/select-route")]
    public async Task<IActionResult> SelectRoute(Guid id, SelectRouteDto dto)
    {
        var userId = GetUserId();
        var item = await _context.Items.FindAsync(id);
        if (item == null) return NotFound();
        if (item.CustomerId != userId) return Forbid();
        if (item.Status != ItemStatus.Assessed) return BadRequest("Item must be assessed before selecting a route.");

        if (!Enum.TryParse<RecoveryRoute>(dto.SelectedRoute, true, out var route))
            return BadRequest("Invalid route selected.");

        item.SelectedRecoveryRoute = route;
        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private ItemDto MapToDto(Item item) => new ItemDto
    {
        Id = item.Id,
        Name = item.Name,
        CategoryId = item.CategoryId,
        Category = item.Category != null ? new CategoryDto { Id = item.Category.Id, Code = item.Category.Code, Name = item.Category.Name } : null,
        Brand = item.Brand,
        Model = item.Model,
        ConditionDescription = item.ConditionDescription,
        Status = item.Status.ToString(),
        SelectedRecoveryRoute = item.SelectedRecoveryRoute?.ToString(),
        Images = item.Images.Select(img => new ItemImageDto { Id = img.Id, ImageUrl = img.ImageUrl, SortOrder = img.SortOrder, IsPrimary = img.IsPrimary }).ToList()
    };
}
