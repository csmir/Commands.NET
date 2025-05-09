namespace Commands.Testing;

/// <summary>
///     A context that is used to execute tests against a command.
/// </summary>
public interface ITestContext : IDisposable
{
    /// <summary>
    ///     Gets a reference to the <see cref="CancellationToken"/> propagated through the test pipeline. When this token is cancelled, the test pipeline will be cancelled.
    /// </summary>
    public CancellationTokenSource CancellationSource { get; }

    /// <summary>
    ///     Gets the command that is being tested.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    ///     Gets the tests that are being executed. This collection is mutable and can be edited before starting to test against the command.
    /// </summary>
    public IList<ITest> Tests { get; }
}
