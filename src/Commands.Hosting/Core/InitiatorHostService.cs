using Microsoft.Extensions.Hosting;

namespace Commands;

/// <summary>
///     Represents a service that hosts a collection of <see cref="ISequenceInitiator"/> instances. This service starts all initiators when the application starts, and stops when the application stops.
/// </summary>
public sealed class InitiatorHostService(IEnumerable<ISequenceInitiator> initiators, IHostApplicationLifetime lifetime) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        lifetime.ApplicationStarted.Register(() =>
        {
            foreach (var initiator in initiators)
                initiator.TryStart();
        });

        lifetime.ApplicationStopping.Register(() =>
        {
            foreach (var initiator in initiators)
                initiator.TryStop();
        });
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
