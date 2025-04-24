
namespace Commands.Testing;

/// <summary>
///     A test collection that can be ran and evaluated per command instance. This class cannot be inherited.
/// </summary>
public sealed class TestCollection : IReadOnlyDictionary<Command, ITestProvider[]>
{
    private readonly Dictionary<Command, ITestProvider[]> _tests;

    /// <summary>
    ///     Gets the number of tests present for this runner.
    /// </summary>
    public int Count { get; }

    /// <inheritdoc />
    public ITestProvider[] this[Command key]
        => _tests[key];

    /// <summary>
    ///     Creates a new instance of <see cref="TestCollection"/> with the specified tests.
    /// </summary>
    /// <param name="tests">The tests, grouped by command that should </param>
    public TestCollection(Dictionary<Command, ITestProvider[]> tests)
    {
        _tests = tests;
        Count = tests.Values.Sum(x => x.Length);
    }

    /// <summary>
    ///     Starts all contained tests sequentially and returns the total result.
    /// </summary>
    /// <param name="creationAction">An action for creating new context when a command is executed. The inbound string represents the tested command name and arguments.</param>
    /// <param name="options">The options to use when running the tests.</param>
    /// <returns>An awaitable <see cref="Task"/> that represents testing operation.</returns>
    public async Task<TestResult[]> Execute(Func<string, ICallerContext> creationAction, CommandOptions? options = null)
    {
        options ??= new CommandOptions();

        var arr = new TestResult[Count];
        var i = 0;

        foreach (var test in _tests)
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

    /// <inheritdoc />
    public bool ContainsKey(Command key)
        => _tests.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(Command key,
#if NET8_0_OR_GREATER
    [MaybeNullWhen(false)]
#endif
    out ITestProvider[] value)
        => _tests.TryGetValue(key, out value);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<Command, ITestProvider[]>> GetEnumerator()
        => _tests.GetEnumerator();

    #region Initializers

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="TestCollection"/>, with the specified commands.
    /// </summary>
    /// <param name="commands">A collection of commands to evaluate. Commands marked with <see cref="TestAttribute"/> will have test providers automatically defined for them.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static TestCollectionProperties From(params Command[] commands)
        => new TestCollectionProperties().AddCommands(commands);

    #endregion

    #region IReadOnlyDictionary<>

    IEnumerable<Command> IReadOnlyDictionary<Command, ITestProvider[]>.Keys
        => _tests.Keys;

    IEnumerable<ITestProvider[]> IReadOnlyDictionary<Command, ITestProvider[]>.Values
        => _tests.Values;

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    #endregion
}