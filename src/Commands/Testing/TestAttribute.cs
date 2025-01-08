namespace Commands.Testing;

/// <summary>
///     An attribute that is used to define a test for a command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class TestAttribute(TestResultType expectedResult = TestResultType.Success) : Attribute, ITestProvider
{
    /// <inheritdoc />
    public TestResultType ExpectedResult { get; set; } = expectedResult;

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
