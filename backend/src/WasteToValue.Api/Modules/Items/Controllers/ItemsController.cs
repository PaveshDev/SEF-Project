using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WasteToValue.Api.Modules.Items.DTOs;
using WasteToValue.Api.Modules.Items.Interfaces;
using WasteToValue.Api.Modules.Items.Services;

namespace WasteToValue.Api.Modules.Items.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController(IItemService itemService) : ControllerBase
{
    private Guid GetOwnerId()
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(claimId, out var id))
            return id;

        throw new System.Security.Authentication.AuthenticationException("User identifier claim is missing or invalid.");
    }

    private ObjectResult HandleException(Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return ex switch
        {
            ItemConcurrencyException => Conflict(new ProblemDetails { Status = 409, Title = "Concurrency conflict", Detail = ex.Message }),
            InvalidOperationException => Conflict(new ProblemDetails { Status = 409, Title = "Invalid operation", Detail = ex.Message }),
            KeyNotFoundException => NotFound(new ProblemDetails { Status = 404, Title = "Not found", Detail = ex.Message }),
            System.Security.Authentication.AuthenticationException => StatusCode(401, new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = ex.Message }),
            UnauthorizedAccessException => StatusCode(403, new ProblemDetails { Status = 403, Title = "Forbidden", Detail = ex.Message }),
            _ => StatusCode(500, new ProblemDetails { Status = 500, Title = "Internal server error", Detail = "An unexpected error occurred." })
        };
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem(CreateItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetOwnerId();
            var response = await itemService.CreateItemAsync(request, ownerId, cancellationToken);
            return CreatedAtAction(nameof(GetItem), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserItems(CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetOwnerId();
            var response = await itemService.GetUserItemsAsync(ownerId, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetItem(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetOwnerId();
            var response = await itemService.GetItemAsync(id, ownerId, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(Guid id, UpdateItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (id != request.Id) return BadRequest(new ProblemDetails { Status = 400, Title = "Bad Request", Detail = "ID mismatch." });

            var ownerId = GetOwnerId();
            var response = await itemService.UpdateItemAsync(request, ownerId, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetOwnerId();
            await itemService.DeleteDraftItemAsync(id, ownerId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/photos")]
    public async Task<IActionResult> AddPhoto(Guid id, AddPhotoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.ItemId = id; // Ensure we use the route ID
            var ownerId = GetOwnerId();
            var response = await itemService.AddPhotoAsync(request, ownerId, cancellationToken);
            return StatusCode(201, response); // Created without a specific GET route for photos
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/condition-answers")]
    public async Task<IActionResult> SubmitConditionAnswers(Guid id, SubmitConditionAnswersRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.ItemId = id;
            var ownerId = GetOwnerId();
            var response = await itemService.SubmitConditionAnswersAsync(request, ownerId, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitItemForAssessment(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetOwnerId();
            var response = await itemService.SubmitItemForAssessmentAsync(id, ownerId, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/assessments")]
    public async Task<IActionResult> CreateAssessment(Guid id, CreateItemAssessmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.ItemId = id;
            var response = await itemService.CreateAssessmentAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetAssessment), new { id = id, assessmentId = response.Id }, response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{id}/assessments/{assessmentId}")]
    public async Task<IActionResult> GetAssessment(Guid id, Guid assessmentId, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = GetOwnerId();
            var response = await itemService.GetAssessmentByIdAsync(assessmentId, ownerId, cancellationToken);
            if (response.ItemId != id) return NotFound(new ProblemDetails { Status = 404, Title = "Not found", Detail = "Assessment not found on this item." });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/assessments/{assessmentId}/evidence")]
    public async Task<IActionResult> AddAssessmentEvidence(Guid id, Guid assessmentId, AddAssessmentEvidenceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.AssessmentId = assessmentId;
            var ownerId = GetOwnerId();
            var response = await itemService.AddEvidenceAsync(request, ownerId, cancellationToken);
            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/clarifications")]
    public async Task<IActionResult> CreateClarification(Guid id, CreateClarificationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.ItemId = id;
            var ownerId = GetOwnerId();
            var response = await itemService.CreateClarificationAsync(request, ownerId, cancellationToken);
            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/clarifications/{clarificationId}/answer")]
    public async Task<IActionResult> AnswerClarification(Guid id, Guid clarificationId, AnswerClarificationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.ClarificationId = clarificationId;
            var ownerId = GetOwnerId();
            var response = await itemService.AnswerClarificationAsync(request, ownerId, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/assessments/{assessmentId}/confirm")]
    public async Task<IActionResult> ConfirmAssessment(Guid id, Guid assessmentId, ConfirmAssessmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.AssessmentId = assessmentId;
            var ownerId = GetOwnerId();
            var response = await itemService.ConfirmAssessmentAsync(request, ownerId, cancellationToken);
            if (response.ItemId != id) return BadRequest(new ProblemDetails { Status = 400, Title = "Bad Request", Detail = "Item mismatch." });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/assessments/{assessmentId}/reassessment")]
    public async Task<IActionResult> RequestReassessment(Guid id, Guid assessmentId, RequestReassessmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            request.AssessmentId = assessmentId;
            var ownerId = GetOwnerId();
            var response = await itemService.RequestReassessmentAsync(request, ownerId, cancellationToken);
            if (response.ItemId != id) return BadRequest(new ProblemDetails { Status = 400, Title = "Bad Request", Detail = "Item mismatch." });
            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
