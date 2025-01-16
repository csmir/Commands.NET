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

    #region Initializers

    /// <inheritdoc cref="From(Command, string?, TestResultType)"/>
    public static TestProviderProperties With
        => new();

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="TestProvider"/>.
    /// </summary>
    /// <param name="command">The command to test.</param>
    /// <param name="arguments">The arguments to test with.</param>
    /// <param name="testResult">The result to test for.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static TestProviderProperties From(Command command, string? arguments = null, TestResultType testResult = TestResultType.Success)
        => new TestProviderProperties().Command(command).Arguments(arguments).Result(testResult);

    #endregion
}