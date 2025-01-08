namespace Commands.Testing;

/// <summary>
///     An attribute that is used to define a test for a command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class TestAttribute : Attribute, ITestProvider
{
    /// <inheritdoc />
    public TestResultType ShouldEvaluateTo { get; set; } = TestResultType.Success;

    /// <inheritdoc />
    public string Arguments { get; set; } = string.Empty;

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
