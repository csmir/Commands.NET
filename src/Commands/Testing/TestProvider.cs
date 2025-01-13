namespace Commands.Testing;

/// <inheritdoc cref="ITestProvider" />
public class TestProvider : ITestProvider
{
    /// <inheritdoc />
    public TestResultType ShouldEvaluateTo { get; }

    /// <inheritdoc />
    public string Arguments { get; }

    /// <summary>
    ///     The command that this test provider is associated with.
    /// </summary>
    public Command Command { get; }

    internal TestProvider(Command command, string arguments, TestResultType shouldEvaluateTo)
    {
        Command = command;
        Arguments = arguments;
        ShouldEvaluateTo = shouldEvaluateTo;
    }

    /// <summary>
    ///     Creates a new test provider with the specified arguments and expected result.
    /// </summary>
    /// <param name="command">The command that should be tested.</param>
    /// <param name="shouldEvaluateTo">The expected result of the evaluation. The test will fail if the outcome is different from this value.</param>
    /// <param name="arguments">A string that should be parsed into valid (or invalid) command input.</param>
    /// <returns>A newly created instance of <see cref="TestProvider"/>.</returns>
    public ITestProvider Create(Command command, TestResultType shouldEvaluateTo, string? arguments = null)
    {
        Assert.NotNull(command, nameof(command));
        Assert.NotNull(arguments, nameof(arguments));
        Assert.NotNull(shouldEvaluateTo, nameof(shouldEvaluateTo));

        if (shouldEvaluateTo > TestResultType.Success && shouldEvaluateTo < TestResultType.InvocationFailure)
            throw new ArgumentOutOfRangeException(nameof(shouldEvaluateTo), shouldEvaluateTo, "The provided value is not a valid test result type.");

        return new TestProvider(command, arguments ?? string.Empty, shouldEvaluateTo);
    }
}