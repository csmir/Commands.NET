namespace Commands.Testing;

/// <summary>
///     An implementation of <see cref="TestRunner"/> that runs tests using a specified <see cref="ICallerContext"/> for each test.
/// </summary>
/// <typeparam name="TContext">The implementation of <see cref="ICallerContext"/> to create in order to run tests.</typeparam>
public class TestRunner<TContext> : TestRunner
    where TContext : ICallerContext, new()
{
    /// <inheritdoc />
    public override event Action<TestResult>? TestCompleted;

    /// <inheritdoc />
    public override event Action<TestResult>? TestFailed;

    internal TestRunner(Dictionary<Command, ITestProvider[]> tests)
        : base(tests) { }

    /// <inheritdoc />
    public override async Task Run(CommandOptions? options = null)
    {
        options ??= new CommandOptions();

        foreach (var test in Tests)
        {
            var caller = new TContext();

            foreach (var provider in test.Value)
            {
                var result = await test.Key.Test(caller, provider, options);

                TestCompleted?.Invoke(result);

                if (!result.Success)
                    TestFailed?.Invoke(result);
            }

            CountCompleted++;
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
    public abstract event Action<TestResult>? TestCompleted;

    /// <summary>
    ///     Triggered when all tests on a command have been completed.
    /// </summary>
    public abstract event Action<TestResult>? TestFailed;

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
    public IReadOnlyDictionary<Command, ITestProvider[]> Tests { get; }

    /// <summary>
    ///     Starts all contained tests sequentially and posts their results to <see cref="TestCompleted"/>.
    /// </summary>
    /// <param name="options">The options to use when running the tests.</param>
    /// <returns>An awaitable <see cref="Task"/> that represents testing operation.</returns>
    public abstract Task Run(CommandOptions? options = null);

    internal TestRunner(Dictionary<Command, ITestProvider[]> tests)
    {
        Tests = tests;
        Count = tests.Count;
        CountCompleted = 0;
    }
}