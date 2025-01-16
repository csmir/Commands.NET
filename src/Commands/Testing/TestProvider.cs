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
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="TestProvider"/>.
    /// </summary>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static TestProviderProperties From()
        => new();
}