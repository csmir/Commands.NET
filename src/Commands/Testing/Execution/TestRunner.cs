namespace Commands.Testing;

/// <summary>
///     A test collection that can be ran and evaluated per command instance. This class cannot be inherited.
/// </summary>
public sealed class TestRunner
{
    /// <summary>
    ///     Gets the number of tests present for this runner.
    /// </summary>
    public int Count { get; }

    /// <summary>
    ///     Gets a collection of commands and tests that are to be ran for that command.
    /// </summary>
    public IReadOnlyDictionary<Command, ITestProvider[]> Tests { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="TestRunner"/> with the specified tests.
    /// </summary>
    /// <param name="tests">The tests, grouped by command that should </param>
    public TestRunner(Dictionary<Command, ITestProvider[]> tests)
    {
        Tests = tests;
        Count = tests.Values.Sum(x => x.Length);
    }

    /// <summary>
    ///     Starts all contained tests sequentially and returns the total result.
    /// </summary>
    /// <param name="creationAction">An action for creating new context when a command is executed. The inbound string represents the tested command name and arguments.</param>
    /// <param name="options">The options to use when running the tests.</param>
    /// <returns>An awaitable <see cref="Task"/> that represents testing operation.</returns>
    public async Task<TestResult[]> Run(Func<string, ICallerContext> creationAction, CommandOptions? options = null)
    {
        options ??= new CommandOptions();

        var arr = new TestResult[Count];
        var i = 0;

        foreach (var test in Tests)
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            foreach (var provider in test.Value)
            {
                arr[i] = await test.Key.Test(creationAction, provider, options);
                i++;
            }
        }

        return arr;
    }

    #region Initializers

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="TestRunner"/>, with the specified commands.
    /// </summary>
    /// <param name="commands">A collection of commands to evaluate. Commands marked with <see cref="TestAttribute"/> will have test providers automatically defined for them.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static TestRunnerProperties From(params Command[] commands)
        => new TestRunnerProperties().Commands(commands);

    #endregion
}