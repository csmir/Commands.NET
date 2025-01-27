using Microsoft.Extensions.Logging;

namespace Commands.Samples;

public sealed class HostedCallerContext(string? input, ILogger logger) : ConsoleContext(input)
{
    public override void Respond(object? response)
        => logger.LogInformation("Response: {}", response);
}
