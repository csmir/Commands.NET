namespace Commands.Testing;

/// <summary>
///     A set of properties for a test runner.
/// </summary>
public sealed class TestCollectionProperties
{
    private readonly List<TestProviderProperties> _tests;
    private readonly List<Command> _commands;

    /// <summary>
    ///     Creates a new instance of <see cref="TestCollectionProperties"/>.
    /// </summary>
    public TestCollectionProperties()
    {
        _tests = [];
        _commands = [];
    }

    /// <summary>
    ///     Adds a test provider to the test runner.
    /// </summary>
    /// <param name="provider">The provider to add.</param>
    /// <returns>The same <see cref="TestCollectionProperties"/> for call-chaining.</returns>
    public TestCollectionProperties AddTest(TestProviderProperties provider)
    {
        Assert.NotNull(provider, nameof(provider));

        _tests.Add(provider);

        return this;
    }

    /// <summary>
    ///     Adds multiple test providers to the test runner.
    /// </summary>
    /// <param name="providers">The providers to add.</param>
    /// <returns>The same <see cref="TestCollectionProperties"/> for call-chaining.</returns>
    public TestCollectionProperties AddTests(params TestProviderProperties[] providers)
    {
        foreach (var provider in providers)
            AddTest(provider);

        return this;
    }

    /// <summary>
    ///     Adds a command to the test runner.
    /// </summary>
    /// <param name="command">The command to add.</param>
    /// <returns>The same <see cref="TestCollectionProperties"/> for call-chaining.</returns>
    public TestCollectionProperties AddCommand(Command command)
    {
        Assert.NotNull(command, nameof(command));

        _commands.Add(command);

        return this;
    }

    /// <summary>
    ///     Adds multiple commands to the test runner.
    /// </summary>
    /// <param name="commands">The commands to add.</param>
    /// <returns>The same <see cref="TestCollectionProperties"/> for call-chaining.</returns>
    public TestCollectionProperties AddCommands(params Command[] commands)
    {
        foreach (var command in commands)
            AddCommand(command);

        return this;
    }

    /// <summary>
    ///     Converts the properties to a new instance of <see cref="TestCollection"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="TestCollection"/>.</returns>
    public TestCollection Create()
    {
        var tests = _commands.ToDictionary(x => x, x => x.Attributes.OfType<ITestProvider>().ToArray());

        var runtimeDefined = _tests.Select(x => x.Create()).GroupBy(x => x.Command);

        foreach (var group in runtimeDefined)
        {
            if (tests.TryGetValue(group.Key, out var value))
                tests[group.Key] = [.. value, .. group];
            else
                tests[group.Key] = [.. group];
        }

        return new TestCollection(tests);
    }
}