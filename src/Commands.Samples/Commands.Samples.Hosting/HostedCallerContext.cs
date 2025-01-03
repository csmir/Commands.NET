using Microsoft.Extensions.Logging;

namespace Commands.Samples;

public sealed class HostedCallerContext(ILogger logger) : ICallerContext
{
    public void Respond(object? response)
        => logger.LogInformation("Response: {}", response);
}
