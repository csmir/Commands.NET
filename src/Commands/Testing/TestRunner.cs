namespace Commands.Testing;

/// <summary>
///     An implementation of <see cref="TestRunner"/> that runs tests using a specified <see cref="ICallerContext"/> for each test.
/// </summary>
/// <typeparam name="TContext">The implementation of <see cref="ICallerContext"/> to create in order to run tests.</typeparam>
public class TestRunner<TContext> : TestRunner
    where TContext : ICallerContext, new()
{
    /// <inheritdoc />
    public override event Action<Command, TestResult>? ResultReceived;

    /// <inheritdoc />
    public override event Action<Command, TestResult>? FailureReceived;

    internal TestRunner(Command command, IEnumerable<ITestProvider> tests)
        : base(command, tests)
    {

    }

    /// <inheritdoc />
    public override async Task Run()
    {
        foreach (var test in Tests)
        {
            var caller = new TContext();

            foreach (var provider in test.Value)
            {
                var result = await provider.Run(caller, test.Key);

                CountCompleted++;

                ResultReceived?.Invoke(test.Key, result);

                if (!result.Success)
                    FailureReceived?.Invoke(test.Key, result);
            }
        }
    }
}

/// <summary>
///     A test collection that can be ran and evaluated per command instance.
/// </summary>
public abstract class TestRunner
{
    /// <summary>
    ///     Triggered when a test result is received.
    /// </summary>
    public abstract event Action<Command, TestResult>? ResultReceived;

    /// <summary>
    ///     Triggered when all tests on a command have been completed.
    /// </summary>
    public abstract event Action<Command, TestResult>? FailureReceived;

    /// <summary>
    ///     Gets the number of tests present for this operation.
    /// </summary>
    public int Count { get; }

    /// <summary>
    ///     Gets the number of tests that have been completed.
    /// </summary>
    public int CountCompleted { get; protected set; }

    /// <summary>
    ///     Gets a collection of commands and tests that are to be ran for that command.
    /// </summary>
    public IReadOnlyDictionary<Command, IEnumerable<ITestProvider>> Tests { get; } 

    /// <summary>
    ///     Starts all contained tests sequentially and posts their results to <see cref="ResultReceived"/>.
    /// </summary>
    public abstract Task Run();

    internal TestRunner(Command command, IEnumerable<ITestProvider> tests)
    {
        Target = command;
        Tests = tests.ToList();
        Count = Tests.Count;
        CountCompleted = 0;
    }

    /// <summary>
    ///     Runs all defined and runtime-defined tests specified on the target command, using a newly created instance of <see cref="ICallerContext"/> that is recreated for each test.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ICallerContext"/> to create for every test.</typeparam>
    /// <param name="command">The command that is targetted to be tested.</param>
    /// <param name="runtimeDefinedTests">The defined tests to run on this command.</param>
    /// <returns>An awaitable <see cref="TestResult"/> containing the result of the test operation. If all tests succeeded, the underlying <see cref="TestResult.ActualResult"/> will be <see cref="TestResultType.Success"/>.</returns>
    public static TestRunner Create<T>(params Command[] commands)
        where T : ICallerContext, new()
    {
        Assert.NotNull(runtimeDefinedTests, nameof(runtimeDefinedTests));

        var definedTests = command.Attributes.OfType<TestAttribute>().Concat(runtimeDefinedTests);

        return new TestRunner<T>(command, definedTests);
    }
}