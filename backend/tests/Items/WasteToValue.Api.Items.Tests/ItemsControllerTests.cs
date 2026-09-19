using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WasteToValue.Api.Modules.Items.Controllers;
using WasteToValue.Api.Modules.Items.DTOs;
using WasteToValue.Api.Modules.Items.Interfaces;
using WasteToValue.Api.Modules.Items.Services;
using Xunit;

namespace WasteToValue.Api.Items.Tests;

public class ItemsControllerTests
{
    private readonly Mock<IItemService> _itemServiceMock;
    private readonly ItemsController _controller;
    private readonly Guid _ownerId = Guid.NewGuid();

    public ItemsControllerTests()
    {
        _itemServiceMock = new Mock<IItemService>();
        _controller = new ItemsController(_itemServiceMock.Object);

        // Setup controller context to mock authenticated user
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, _ownerId.ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateItem_Returns201Created()
    {
        // Arrange
        var request = new CreateItemRequest { Title = "Test", Category = "Electronics", LocationArea = "NY" };
        var response = new ItemResponse { Id = Guid.NewGuid(), Title = "Test" };
        _itemServiceMock.Setup(s => s.CreateItemAsync(request, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreateItem(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(response, createdResult.Value);
    }
    [Fact]
    public async Task GetUserItems_Returns200OK_WithItems()
    {
        // Arrange
        var response = new List<ItemSummaryResponse> 
        { 
            new ItemSummaryResponse { Id = Guid.NewGuid(), Title = "Item 1" } 
        };
        _itemServiceMock.Setup(s => s.GetUserItemsAsync(_ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetUserItems(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task GetUserItems_Returns200OK_WithEmptyList()
    {
        // Arrange
        var response = new List<ItemSummaryResponse>();
        _itemServiceMock.Setup(s => s.GetUserItemsAsync(_ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetUserItems(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(response, okResult.Value);
    }

#if !DEBUG
    [Fact]
    public async Task GetUserItems_Unauthenticated_Returns401()
    {
        // Arrange
        var unauthenticatedController = new ItemsController(_itemServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // No User
            }
        };

        // Act
        var result = await unauthenticatedController.GetUserItems(CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, objectResult.StatusCode);
    }
#endif
    [Fact]
    public async Task GetItem_Returns200OK()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var response = new ItemResponse { Id = itemId };
        _itemServiceMock.Setup(s => s.GetItemAsync(itemId, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetItem(itemId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_WithValidVersion_Returns200OK()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new UpdateItemRequest { Id = itemId, Version = 1 };
        var response = new ItemResponse { Id = itemId, Version = 2 };
        _itemServiceMock.Setup(s => s.UpdateItemAsync(request, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateItem(itemId, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_WithConcurrencyConflict_Returns409Conflict()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new UpdateItemRequest { Id = itemId, Version = 1 };
        _itemServiceMock.Setup(s => s.UpdateItemAsync(request, _ownerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ItemConcurrencyException("Conflict"));

        // Act
        var result = await _controller.UpdateItem(itemId, request, CancellationToken.None);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflictResult.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(conflictResult.Value);
        Assert.Equal("Concurrency conflict", problem.Title);
    }

    [Fact]
    public async Task DeleteItem_Returns204NoContent()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _itemServiceMock.Setup(s => s.DeleteDraftItemAsync(itemId, _ownerId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteItem(itemId, CancellationToken.None);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(204, noContentResult.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_InvalidIdMismatch_Returns400BadRequest()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new UpdateItemRequest { Id = Guid.NewGuid() }; // mismatch

        // Act
        var result = await _controller.UpdateItem(itemId, request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateAssessment_Returns201Created()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new CreateItemAssessmentRequest { ItemId = itemId };
        var response = new ItemAssessmentResponse { Id = Guid.NewGuid(), ItemId = itemId };
        _itemServiceMock.Setup(s => s.CreateAssessmentAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreateAssessment(itemId, request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task AddAssessmentEvidence_CrossItemFailure_Returns409Conflict()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var assessmentId = Guid.NewGuid();
        var request = new AddAssessmentEvidenceRequest { AssessmentId = assessmentId, PhotoId = Guid.NewGuid() };
        _itemServiceMock.Setup(s => s.AddEvidenceAsync(request, _ownerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("The photo must belong to the same item as the assessment."));

        // Act
        var result = await _controller.AddAssessmentEvidence(itemId, assessmentId, request, CancellationToken.None);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(409, conflictResult.StatusCode);
    }

    [Fact]
    public async Task ConfirmAssessment_Returns200OK()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var assessmentId = Guid.NewGuid();
        var request = new ConfirmAssessmentRequest { AssessmentId = assessmentId, OwnerConfirmation = true };
        var response = new ItemAssessmentResponse { Id = assessmentId, ItemId = itemId };
        
        _itemServiceMock.Setup(s => s.ConfirmAssessmentAsync(request, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.ConfirmAssessment(itemId, assessmentId, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task RequestReassessment_Returns200OK()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var assessmentId = Guid.NewGuid();
        var request = new RequestReassessmentRequest { AssessmentId = assessmentId };
        var response = new ItemAssessmentResponse { Id = assessmentId, ItemId = itemId };
        
        _itemServiceMock.Setup(s => s.RequestReassessmentAsync(request, _ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.RequestReassessment(itemId, assessmentId, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetItem_MissingResource_Returns404NotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _itemServiceMock.Setup(s => s.GetItemAsync(itemId, _ownerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Item not found."));

        // Act
        var result = await _controller.GetItem(itemId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
}
