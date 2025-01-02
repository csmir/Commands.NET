namespace Commands;

/// <summary>
///     A generator for command execution scopes, listening to data within the provided collection of <see cref="SourceProvider"/> to run a new command.
/// </summary>
public interface ISequenceInitiator
{
    /// <summary>
    ///     Gets a token source that can cancel the parallel source loop.
    /// </summary>
    public CancellationTokenSource CancellationSource { get; }

    /// <summary>
    ///     Gets the components available for execution for this initiator.
    /// </summary>
    public IComponentTree Tree { get; }

    /// <summary>
    ///     Gets a collection of source resolvers that are used to listen for command input.
    /// </summary>
    public IEnumerable<SourceProvider> Resolvers { get; }

    /// <summary>
    ///     Makes an attempt to start command input entering the application flow.
    /// </summary>
    public void TryStart();

    /// <summary>
    ///     Makes an attempt to stop command input entering the application flow.
    /// </summary>
    public void TryStop();

    /// <summary>
    ///     Makes an attempt to pause command input entering the application flow, and returns a value indicating whether the operation was successful.
    /// </summary>
    /// <returns><see langword="true"/> if all source resolvers were succesfully paused. <see langword="false"/> if one or more failed to pause.</returns>
    public bool TryPause();

    /// <summary>
    ///     Makes an attempt to unpause command input entering the application flow, and returns a value indicating whether the operation was successful.
    /// </summary>
    /// <returns><see langword="true"/> if all source resolvers succesfully unpaused. <see langword="false"/> if one or more failed to unpause.</returns>
    public bool TryUnpause();
}
