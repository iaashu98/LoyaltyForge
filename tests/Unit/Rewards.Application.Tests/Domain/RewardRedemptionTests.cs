using Xunit;
using FluentAssertions;
using Rewards.Domain.Entities;

namespace Rewards.Application.Tests.Domain;

public class RewardRedemptionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _rewardId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsRedemptionWithPendingStatus()
    {
        // Act
        var redemption = RewardRedemption.Create(
            _tenantId,
            _userId,
            _rewardId,
            "idempotency-key-1",
            pointsSpent: 50);

        // Assert
        redemption.Should().NotBeNull();
        redemption.Id.Should().NotBeEmpty();
        redemption.TenantId.Should().Be(_tenantId);
        redemption.UserId.Should().Be(_userId);
        redemption.RewardId.Should().Be(_rewardId);
        redemption.IdempotencyKey.Should().Be("idempotency-key-1");
        redemption.PointsSpent.Should().Be(50);
        redemption.Status.Should().Be("pending");
        redemption.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarkFulfilled_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var redemption = RewardRedemption.Create(_tenantId, _userId, _rewardId, "key-1", 50);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        redemption.MarkFulfilled();

        // Assert
        redemption.Status.Should().Be("fulfilled");
        redemption.FulfilledAt.Should().NotBeNull();
        redemption.FulfilledAt.Should().BeOnOrAfter(beforeUpdate);
        redemption.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void MarkFulfilled_WithFulfillmentData_StoresData()
    {
        // Arrange
        var redemption = RewardRedemption.Create(_tenantId, _userId, _rewardId, "key-1", 50);

        // Act
        redemption.MarkFulfilled(fulfillmentData: "{\"code\":\"ABC123\"}", externalReference: "EXT-REF-1");

        // Assert
        redemption.Status.Should().Be("fulfilled");
        redemption.FulfillmentData.Should().Be("{\"code\":\"ABC123\"}");
        redemption.ExternalReference.Should().Be("EXT-REF-1");
    }

    [Fact]
    public void MarkFailed_UpdatesStatusAndTimestamp()
    {
        // Arrange
        var redemption = RewardRedemption.Create(_tenantId, _userId, _rewardId, "key-1", 50);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        redemption.MarkFailed();

        // Assert
        redemption.Status.Should().Be("failed");
        redemption.UpdatedAt.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public void MarkCancelled_UpdatesStatus()
    {
        // Arrange
        var redemption = RewardRedemption.Create(_tenantId, _userId, _rewardId, "key-1", 50);

        // Act
        redemption.MarkCancelled();

        // Assert
        redemption.Status.Should().Be("cancelled");
    }

    [Fact]
    public void MarkExpired_UpdatesStatus()
    {
        // Arrange
        var redemption = RewardRedemption.Create(_tenantId, _userId, _rewardId, "key-1", 50);

        // Act
        redemption.MarkExpired();

        // Assert
        redemption.Status.Should().Be("expired");
    }
}
