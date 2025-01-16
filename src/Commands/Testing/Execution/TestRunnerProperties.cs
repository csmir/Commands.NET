namespace Commands.Testing;

/// <summary>
///     A set of properties for a test runner.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class TestRunnerProperties<T>
    where T : ICallerContext, new()
{
    private readonly List<TestProviderProperties> _tests;
    private readonly List<Command> _commands;

    /// <summary>
    ///     Creates a new instance of <see cref="TestRunnerProperties{T}"/>.
    /// </summary>
    public TestRunnerProperties()
    {
        _tests = [];
        _commands = [];
    }

    /// <summary>
    ///     Adds a test provider to the test runner.
    /// </summary>
    /// <param name="provider">The provider to add.</param>
    /// <returns>The same <see cref="TestRunnerProperties{T}"/> for call-chaining.</returns>
    public TestRunnerProperties<T> Test(TestProviderProperties provider)
    {
        Assert.NotNull(provider, nameof(provider));

        _tests.Add(provider);

        return this;
    }

    /// <summary>
    ///     Adds multiple test providers to the test runner.
    /// </summary>
    /// <param name="providers">The providers to add.</param>
    /// <returns>The same <see cref="TestRunnerProperties{T}"/> for call-chaining.</returns>
    public TestRunnerProperties<T> Tests(params TestProviderProperties[] providers)
    {
        foreach (var provider in providers)
            Test(provider);

        return this;
    }

    /// <summary>
    ///     Adds a command to the test runner.
    /// </summary>
    /// <param name="command">The command to add.</param>
    /// <returns>The same <see cref="TestRunnerProperties{T}"/> for call-chaining.</returns>
    public TestRunnerProperties<T> Command(Command command)
    {
        Assert.NotNull(command, nameof(command));

        _commands.Add(command);

        return this;
    }

    /// <summary>
    ///     Adds multiple commands to the test runner.
    /// </summary>
    /// <param name="commands">The commands to add.</param>
    /// <returns>The same <see cref="TestRunnerProperties{T}"/> for call-chaining.</returns>
    public TestRunnerProperties<T> Commands(params Command[] commands)
    {
        foreach (var command in commands)
            Command(command);

        return this;
    }

    /// <summary>
    ///     Converts the properties to a new instance of <see cref="TestRunner{TContext}"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="TestRunner{TContext}"/>.</returns>
    public TestRunner<T> ToRunner()
    {
        var tests = _commands.ToDictionary(x => x, x => x.Attributes.OfType<ITestProvider>().ToArray());

        var runtimeDefined = _tests.Select(x => x.ToTest()).GroupBy(x => x.Command);

        foreach (var group in runtimeDefined)
        {
            if (tests.TryGetValue(group.Key, out var value))
                tests[group.Key] = [.. value, .. group];
            else
                tests[group.Key] = [.. group];
        }

        return new TestRunner<T>(tests);
    }
}