namespace Rewards.Domain.Enums;

/// <summary>
/// Status of a redemption request.
/// </summary>
public enum RedemptionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
