namespace Commands.Testing;

/// <inheritdoc cref="ITestProvider" />
public class TestProvider : ITestProvider
{
    /// <inheritdoc />
    public TestResultType ShouldEvaluateTo { get; }

    /// <inheritdoc />
    public string Arguments { get; }

    internal TestProvider(string arguments, TestResultType shouldEvaluateTo)
    {
        Arguments = arguments;
        ShouldEvaluateTo = shouldEvaluateTo;
    }

    /// <inheritdoc />
    public ValueTask<TestResult> Run<T>(T caller, Command command, CommandOptions? options = null)
        where T : ICallerContext
    {
        Assert.NotNull(caller, nameof(caller));
        Assert.NotNull(command, nameof(command));

        options ??= new CommandOptions();

        return command.Test(caller, this, options);
    }

    /// <summary>
    ///     Creates a new test provider with the specified arguments and expected result.
    /// </summary>
    /// <param name="shouldEvaluateTo">The expected result of the evaluation. The test will fail if the outcome is different from this value.</param>
    /// <param name="arguments">A string that should be parsed into valid (or invalid) command input.</param>
    /// <returns>A newly created instance of <see cref="TestProvider"/>.</returns>
    public ITestProvider Create(TestResultType shouldEvaluateTo, string? arguments = null)
    {
        Assert.NotNull(arguments, nameof(arguments));
        Assert.NotNull(shouldEvaluateTo, nameof(shouldEvaluateTo));

        if (shouldEvaluateTo > TestResultType.Success && shouldEvaluateTo < TestResultType.InvocationFailure)
            throw new ArgumentOutOfRangeException(nameof(shouldEvaluateTo), shouldEvaluateTo, "The provided value is not a valid test result type.");

        return new TestProvider(arguments ?? string.Empty, shouldEvaluateTo);
    }
}