namespace Commands.Testing;

public sealed class TestRunnerProperties<T> : TestRunnerProperties
    where T : ICallerContext, new()
{
    private readonly List<TestProviderProperties> _tests;
    private readonly List<Command> _commands;

    public TestRunnerProperties()
    {
        _tests = [];
        _commands = [];
    }

    public TestRunnerProperties<T> Test(TestProviderProperties provider)
    {
        Assert.NotNull(provider, nameof(provider));

        _tests.Add(provider);

        return this;
    }

    public TestRunnerProperties<T> Tests(params TestProviderProperties[] providers)
    {
        foreach (var provider in providers)
            Test(provider);

        return this;
    }

    public TestRunnerProperties<T> Command(Command command)
    {
        Assert.NotNull(command, nameof(command));

        _commands.Add(command);

        return this;
    }

    public TestRunnerProperties<T> Commands(params Command[] commands)
    {
        foreach (var command in commands)
            Command(command);

        return this;
    }

    public override TestRunner ToRunner()
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

public abstract class TestRunnerProperties
{
    public abstract TestRunner ToRunner();
}