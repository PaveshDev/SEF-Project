using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.Agents.Tools;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Services;

using Xunit;

namespace WasteToValue.Tests.Collections;

public sealed class AgenticCollectionPlanningTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task PlanningAgent_ExecutesToolPipeline_CreatesAndPersistsProposal()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var slotService = new CollectionSlotService(db);
        var travelService = new TravelEstimateService();
        var tools = new CollectionAgentTools(slotService, travelService);
        var agent = new CollectionAgentService(db, tools);

        var pickupId = Guid.NewGuid();

        // Act
        var proposal = await agent.PrepareCollectionPlanAsync(pickupId);

        // Assert
        Assert.NotNull(proposal);
        Assert.Equal(pickupId, proposal.PickupRequestId);
        Assert.NotEmpty(proposal.ConstraintChecks);
        Assert.Contains(proposal.ConstraintChecks, c => c.Constraint == "Vehicle capacity" && c.Passed);

        var planEntity = await db.PickupPlans.FirstOrDefaultAsync(p => p.Id == proposal.ProposalId);
        Assert.NotNull(planEntity);
        Assert.Contains("workflow", planEntity.TravelEstimate);
    }

    [Fact]
    public async Task ReplanningAgent_ExecutesRescheduleWorkflow_CreatesProposal()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var slotService = new CollectionSlotService(db);
        var travelService = new TravelEstimateService();
        var tools = new CollectionAgentTools(slotService, travelService);
        var agent = new CollectionAgentService(db, tools);

        var pickupId = Guid.NewGuid();
        var rescheduleReq = new RescheduleRequestDto(pickupId, Guid.NewGuid(), "Vehicle breakdown");

        // Act
        var proposal = await agent.RescheduleAsync(rescheduleReq);

        // Assert
        Assert.NotNull(proposal);
        Assert.Equal(pickupId, proposal.PickupRequestId);
        Assert.Contains("Vehicle breakdown", proposal.ReasonForRecommendation);

        var planEntity = await db.PickupPlans.FirstOrDefaultAsync(p => p.Id == proposal.ProposalId);
        Assert.NotNull(planEntity);
    }

    [Fact]
    public async Task PlanningAgent_WhenVehicleCapacityExceeded_ReturnsManualReviewSafely()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var slotService = new CollectionSlotService(db);
        var travelService = new TravelEstimateService();
        var tools = new CollectionAgentTools(slotService, travelService);

        var ctx = new WasteToValue.Api.Modules.Collections.Agents.AgentWorkflowContext(
            Guid.NewGuid(), Guid.NewGuid(), "PLAN", "TEST", "IN_PROGRESS",
            new List<WasteToValue.Api.Modules.Collections.Agents.ToolCallLog>(),
            new List<ConstraintCheckDto>(), new List<string>(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        // Act - Heavy item (3000 kg) on Three Wheeler (300 kg cap)
        var check = await tools.CheckVehicleCapacityAsync("Three Wheeler", 3000m, ctx);

        // Assert
        Assert.False(check.Passed);
        Assert.Contains("exceeds", check.Detail, StringComparison.OrdinalIgnoreCase);

        var proposal = tools.ProposePickup(Guid.NewGuid(), null, null, new List<string>(), ctx.ValidationChecks, ctx);
        Assert.Equal("MANUAL_REVIEW", proposal.FeasibilityStatus);
    }

    [Fact]
    public async Task HumanApprovalSafeguard_AIProposalDoesNotConfirmPickup_UntilStaffApproves()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var slotService = new CollectionSlotService(db);
        var travelService = new TravelEstimateService();
        var tools = new CollectionAgentTools(slotService, travelService);
        var agent = new CollectionAgentService(db, tools);

        var pickupId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();

        var pickup = new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = ownerId,
            Status = "PENDING_APPROVAL",
            IdempotencyKey = "key-safeguard",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.PickupRequests.Add(pickup);
        await db.SaveChangesAsync();

        // Act 1: Agent creates plan proposal
        var proposal = await agent.PrepareCollectionPlanAsync(pickupId);

        // Assert 1: Pickup status is STILL PENDING_APPROVAL (AI did NOT directly confirm)
        var unapprovedPickup = await db.PickupRequests.FindAsync(pickupId);
        Assert.NotNull(unapprovedPickup);
        Assert.Equal("PENDING_APPROVAL", unapprovedPickup.Status);

        // Act 2: Human staff approves proposal
        var decision = new ApprovalDecisionDto(proposal.ProposalId, "APPROVED", "Approved by staff", Guid.NewGuid());
        var approvedProposal = await agent.ApproveProposalAsync(decision);

        // Assert 2: ONLY AFTER human staff approval is status updated to CONFIRMED and OTP generated
        Assert.NotNull(approvedProposal);
        Assert.Equal("APPROVED", approvedProposal.FeasibilityStatus);

        var confirmedPickup = await db.PickupRequests.FindAsync(pickupId);
        Assert.NotNull(confirmedPickup);
        Assert.Equal("CONFIRMED", confirmedPickup.Status);
        Assert.False(string.IsNullOrWhiteSpace(confirmedPickup.VerificationCode));
    }

    [Fact]
    public async Task ApproveProposal_WhenSlotStaleOrFull_RejectsProposalAndRequiresReplanning()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var slotService = new CollectionSlotService(db);
        var travelService = new TravelEstimateService();
        var tools = new CollectionAgentTools(slotService, travelService);
        var agent = new CollectionAgentService(db, tools);

        var slotId = Guid.NewGuid();
        var pickupId = Guid.NewGuid();

        db.CollectionSlots.Add(new CollectionSlot
        {
            Id = slotId,
            StartsAt = DateTimeOffset.UtcNow,
            EndsAt = DateTimeOffset.UtcNow.AddHours(2),
            ServiceArea = "Colombo",
            Capacity = 1,
            ReservedCount = 1,
            Status = "BOOKED", // Slot is now FULL/BOOKED!
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = slotId,
            OwnerId = Guid.NewGuid(),
            Status = "PENDING_APPROVAL",
            IdempotencyKey = "key-stale",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        db.PickupPlans.Add(new PickupPlan
        {
            Id = Guid.NewGuid(),
            MatchId = pickupId,
            CollectionSlotId = slotId,
            ProposedStart = DateTimeOffset.UtcNow,
            ProposedEnd = DateTimeOffset.UtcNow.AddHours(2),
            FeasibilityStatus = "FEASIBLE",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        var plan = await db.PickupPlans.FirstAsync();
        var decision = new ApprovalDecisionDto(plan.Id, "APPROVED", "Try approving stale slot", Guid.NewGuid());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => agent.ApproveProposalAsync(decision));
        Assert.Contains("no longer available", ex.Message);

        var rejectedPlan = await db.PickupPlans.FindAsync(plan.Id);
        Assert.NotNull(rejectedPlan);
        Assert.Equal("REJECTED", rejectedPlan.FeasibilityStatus);
    }

    [Fact]
    public async Task AgentTools_ValidatesInputAndFailsSafelyWhenTravelApiFails()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var slotService = new CollectionSlotService(db);

        // Failing travel estimate service mock
        var failingTravelService = new FailingTravelEstimateService();
        var tools = new CollectionAgentTools(slotService, failingTravelService);

        var ctx = new WasteToValue.Api.Modules.Collections.Agents.AgentWorkflowContext(
            Guid.NewGuid(), Guid.NewGuid(), "PLAN", "TEST", "IN_PROGRESS",
            new List<WasteToValue.Api.Modules.Collections.Agents.ToolCallLog>(),
            new List<ConstraintCheckDto>(), new List<string>(),
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        // Act: Invoke travel estimate tool when API throws exception
        var result = await tools.GetTravelEstimateAsync("Colombo", "Kandy", ctx);

        // Assert: Tool catches exception, logs error, returns null, marks check passed=false (safe failure)
        Assert.Null(result);
        Assert.NotEmpty(ctx.Errors);
        Assert.Contains(ctx.ValidationChecks, c => c.Constraint == "Travel feasibility" && !c.Passed);
    }
}

public sealed class FailingTravelEstimateService : WasteToValue.Api.Modules.Collections.Interfaces.ITravelEstimateService
{
    public Task<WasteToValue.Api.Modules.Collections.Interfaces.TravelEstimateResult?> GetEstimateAsync(string origin, string destination, CancellationToken ct = default)
    {
        throw new InvalidOperationException("External maps routing service timeout / unreachable.");
    }
}
