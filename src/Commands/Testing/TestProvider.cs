namespace Commands.Testing;

/// <inheritdoc cref="ITestProvider" />
public class TestProvider : ITestProvider
{
    /// <inheritdoc />
    public TestResultType ExpectedResult { get; set; } = TestResultType.Success;

    /// <inheritdoc />
    public KeyValuePair<string, object?>[] Arguments { get; set; } = [];

    /// <inheritdoc />
    public ValueTask<TestResult> Run<T>(T caller, Command command, CommandOptions? options = null)
        where T : ICallerContext
    {
        Assert.NotNull(caller, nameof(caller));
        Assert.NotNull(command, nameof(command));

        options ??= new CommandOptions();

        return command.Test(caller, this, options);
    }
}