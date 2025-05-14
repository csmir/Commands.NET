namespace Commands.Testing;

/// <summary>
///     An implementation of <see cref="ITestContext"/> that is used to execute tests against a command.
/// </summary>
/// <param name="command">The command that is represented by this context.</param>
public class TestContext(Command command) : ITestContext
{
    /// <summary>
    ///     Gets the command that is being tested.
    /// </summary>
    public Command Command { get; internal set; } = command;

    /// <summary>
    ///     Gets a collection of tests that target the command in this context.
    /// </summary>
    public IList<ITest> Tests { get; internal set; } = null!;

    /// <summary>
    ///     Gets a reference to the <see cref="CancellationTokenSource"/> propagated through the test pipeline. When this token is cancelled, the test pipeline will be cancelled.
    /// </summary>
    public CancellationTokenSource CancellationSource { get; internal set; } = null!;

    /// <inheritdoc />
    public virtual void Dispose()
    {

    }
}
