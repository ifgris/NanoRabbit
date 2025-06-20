using Microsoft.Extensions.Logging;

namespace NanoRabbit;

/// <summary>
/// Message handler interface.
/// </summary>
public interface IAsyncMessageHandler
{
    /// <summary>
    /// Handles the received message.
    /// </summary>
    /// <param name="messageBody">The message body (raw bytes).</param>
    /// <param name="routingKey">The routing key of the message (if applicable).</param>
    /// <param name="correlationId">The correlation ID of the message (if applicable).</param>
    /// <returns>Returns true if the message was handled successfully; otherwise, false.</returns>
    Task HandleMessageAsync(byte[] messageBody, string? routingKey = null, string? correlationId = null);
}

/// <summary>
/// Default message handler
/// </summary>
public class DefaultAsyncMessageHandler : IAsyncMessageHandler
{
    private readonly ILogger<DefaultAsyncMessageHandler> _logger;

    /// <summary>
    /// Default message handler constructor
    /// </summary>
    /// <param name="logger"></param>
    public DefaultAsyncMessageHandler(ILogger<DefaultAsyncMessageHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Implement of message handler
    /// </summary>
    /// <param name="messageBody"></param>
    /// <param name="routingKey"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public async Task HandleMessageAsync(byte[] messageBody, string? routingKey = null, string? correlationId = null)
    {
        try
        {
            // Deserialize the message
            var messageString = System.Text.Encoding.UTF8.GetString(messageBody);
            _logger.LogInformation("Received message (Handler): RoutingKey='{RoutingKey}', CorrelationId='{CorrelationId}', Body='{MessageBody}'",
                routingKey, correlationId, messageString);

            await Task.Delay(100);

            _logger.LogInformation("Message handled successfully (Handler).");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while handling the message (Handler). CorrelationId='{CorrelationId}'", correlationId);
        }
    }
}