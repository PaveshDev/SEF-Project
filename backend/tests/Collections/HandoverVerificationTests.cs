using Microsoft.EntityFrameworkCore;
using WasteToValue.Api.Infrastructure.Persistence;
using WasteToValue.Api.Modules.Collections.DTOs;
using WasteToValue.Api.Modules.Collections.Entities;
using WasteToValue.Api.Modules.Collections.Services;
using WasteToValue.Api.Modules.Collections.Validators;
using Xunit;

namespace WasteToValue.Tests.Collections;

public sealed class HandoverVerificationTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task VerifyCode_WithCorrectOTP_SucceedsAndCreatesProof()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new HandoverService(db);
        var pickupId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var plainOtp = "123456";
        var verificationString = HandoverCodeHasher.FormatVerificationString(pickupId, plainOtp, TimeSpan.FromHours(1));

        var pickup = new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = actorId,
            Status = "ASSIGNED",
            VerificationCode = verificationString,
            IdempotencyKey = "key-1",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.PickupRequests.Add(pickup);
        await db.SaveChangesAsync();

        var request = new SubmitHandoverCodeRequest(plainOtp, actorId);

        // Act
        var result = await service.VerifyCodeAsync(pickupId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pickupId, result.PickupRequestId);
        Assert.Equal("ONE_TIME_CODE", result.ProofType);
        Assert.Equal(actorId, result.VerifiedBy);

        var updatedPickup = await db.PickupRequests.FindAsync(pickupId);
        Assert.NotNull(updatedPickup);
        Assert.Equal("COLLECTED", updatedPickup.Status);

        var proof = await db.HandoverProofs.FirstOrDefaultAsync(h => h.PickupRequestId == pickupId);
        Assert.NotNull(proof);

        var auditEvent = await db.PickupEvents.FirstOrDefaultAsync(e => e.PickupRequestId == pickupId && e.EventType == "HANDOVER_VERIFIED");
        Assert.NotNull(auditEvent);
    }

    [Fact]
    public async Task VerifyCode_WithWrongOTP_FailsAndIncrementsFailedAttempts()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new HandoverService(db);
        var pickupId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var correctOtp = "123456";
        var wrongOtp = "654321";
        var verificationString = HandoverCodeHasher.FormatVerificationString(pickupId, correctOtp, TimeSpan.FromHours(1));

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = actorId,
            Status = "ASSIGNED",
            VerificationCode = verificationString,
            IdempotencyKey = "key-2",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var request = new SubmitHandoverCodeRequest(wrongOtp, actorId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyCodeAsync(pickupId, request));
        Assert.Contains("Invalid one-time code", ex.Message);

        var auditEvent = await db.PickupEvents.FirstOrDefaultAsync(e => e.PickupRequestId == pickupId && e.EventType == "HANDOVER_FAILED");
        Assert.NotNull(auditEvent);

        var updatedPickup = await db.PickupRequests.FindAsync(pickupId);
        Assert.NotNull(updatedPickup);
        Assert.Contains("|1|0", updatedPickup.VerificationCode); // 1 failed attempt, 0 used
    }

    [Fact]
    public async Task VerifyCode_WithExpiredOTP_FailsWithExpiredErrorMessage()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new HandoverService(db);
        var pickupId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var plainOtp = "123456";
        // Create verification string expired 10 minutes ago
        var expiredString = HandoverCodeHasher.FormatVerificationString(pickupId, plainOtp, TimeSpan.FromMinutes(-10));

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = actorId,
            Status = "ASSIGNED",
            VerificationCode = expiredString,
            IdempotencyKey = "key-3",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var request = new SubmitHandoverCodeRequest(plainOtp, actorId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyCodeAsync(pickupId, request));
        Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyCode_WithReusedOTP_FailsWithAlreadyUsedErrorMessage()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new HandoverService(db);
        var pickupId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var plainOtp = "123456";
        var verificationString = HandoverCodeHasher.FormatVerificationString(pickupId, plainOtp, TimeSpan.FromHours(1));

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = actorId,
            Status = "ASSIGNED",
            VerificationCode = verificationString,
            IdempotencyKey = "key-4",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var request = new SubmitHandoverCodeRequest(plainOtp, actorId);

        // First verification succeeds
        var firstResult = await service.VerifyCodeAsync(pickupId, request);
        Assert.NotNull(firstResult);

        // Second verification with same OTP fails
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyCodeAsync(pickupId, request));
        Assert.Contains("already been used", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerifyCode_WithTooManyFailedAttempts_FailsWithMaxAttemptsExceeded()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new HandoverService(db);
        var pickupId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var correctOtp = "123456";
        var wrongOtp = "999999";
        var verificationString = HandoverCodeHasher.FormatVerificationString(pickupId, correctOtp, TimeSpan.FromHours(1));

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = actorId,
            Status = "ASSIGNED",
            VerificationCode = verificationString,
            IdempotencyKey = "key-5",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var request = new SubmitHandoverCodeRequest(wrongOtp, actorId);

        // Fail 5 times
        for (int i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyCodeAsync(pickupId, request));
        }

        // 6th attempt with correct OTP fails because attempts exceeded
        var correctRequest = new SubmitHandoverCodeRequest(correctOtp, actorId);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyCodeAsync(pickupId, correctRequest));
        Assert.Contains("Maximum verification attempts exceeded", ex.Message);
    }

    [Fact]
    public async Task VerifyCode_WithInvalidPickupStatus_FailsWithStatusError()
    {
        // Arrange
        using var db = CreateInMemoryDbContext();
        var service = new HandoverService(db);
        var pickupId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var plainOtp = "123456";
        var verificationString = HandoverCodeHasher.FormatVerificationString(pickupId, plainOtp);

        db.PickupRequests.Add(new PickupRequest
        {
            Id = pickupId,
            RecoveryProposalId = Guid.NewGuid(),
            CollectionSlotId = Guid.NewGuid(),
            OwnerId = actorId,
            Status = "CANCELLED", // Ineligible status!
            VerificationCode = verificationString,
            IdempotencyKey = "key-6",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var request = new SubmitHandoverCodeRequest(plainOtp, actorId);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyCodeAsync(pickupId, request));
        Assert.Contains("not allowed for pickup with status CANCELLED", ex.Message);
    }
}
