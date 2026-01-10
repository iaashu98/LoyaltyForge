using LoyaltyForge.Contracts.Commands;

namespace LoyaltyForge.Messaging.RabbitMQ;

/// <summary>
/// Interface for handling commands.
/// </summary>
/// <typeparam name="TCommand">Type of command to handle</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : IntegrationCommand
{
    /// <summary>
    /// Handles the command and returns a result.
    /// </summary>
    Task<CommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of command execution.
/// </summary>
/// <param name="Success">Whether the command was successfully processed</param>
/// <param name="Error">Error message if failed</param>
/// <param name="Data">Optional result data</param>
public record CommandResult(bool Success, string? Error = null, object? Data = null);
