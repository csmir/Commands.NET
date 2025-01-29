using Microsoft.Extensions.Logging;

namespace Commands.Samples;

// In order to format responses according to the logger's stream format, we must pass it along and append the response to the log.
public sealed class HostedCallerContext(string? input, ILogger logger) : ConsoleContext(input)
{
    // By overriding the response method, we can use the logger instead of the normal console out-stream.
    public override void Respond(object? response)
        => logger.LogInformation("Response: {}", response);
}
