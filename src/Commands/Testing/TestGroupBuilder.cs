namespace Commands.Testing;

/// <summary>
///     Represents a collection of properties with which a new instance of <see cref="TestGroup"/> can be constructed.
/// </summary>
public sealed class TestGroupBuilder
{
    private Command _command;
    private readonly List<ITest> _tests;
    
    /// <summary>
    ///     Creates a new instance of <see cref="TestGroupBuilder"/> to create a <see cref="TestGroup"/> from.
    /// </summary>
    public TestGroupBuilder()
    {
        _tests = [];
        _command = null!;
    }

    /// <summary>
    ///     Sets the <see cref="Command"/>. Calling this method multiple times replaces the old value with the new one.
    /// </summary>
    /// <param name="command">The <see cref="Command"/> to provide as the target of the <see cref="TestGroup"/> these properties create.</param>
    /// <returns>The same <see cref="TestGroupBuilder"/> for call-chaining.</returns>
    public TestGroupBuilder WithCommand(Command command)
    {
        Assert.NotNull(command, nameof(command));

        _command = command;

        return this;
    }

    /// <summary>
    ///     Adds an implementation of <see cref="ITest"/> to the properties.
    /// </summary>
    /// <param name="test">The test to add.</param>
    /// <returns>The same <see cref="TestGroupBuilder"/> for call-chaining.</returns>
    public TestGroupBuilder AddTest(ITest test)
    {
        Assert.NotNull(test, nameof(test));

        _tests.Add(test);

        return this;
    }

    /// <summary>
    ///     Adds the constructed <see cref="TestProperties"/> to the properties.
    /// </summary>
    /// <param name="properties">The properties to add.</param>
    /// <returns>The same <see cref="TestGroupBuilder"/> for call-chaining.</returns>
    public TestGroupBuilder AddTest(TestProperties properties)
    {
        Assert.NotNull(properties, nameof(properties));

        _tests.Add(properties.ToTest());

        return this;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="TestProperties"/> and configures it using the provided action, adding it to the properties.
    /// </summary>
    /// <param name="configureProperties">The configuration for the properties to add.</param>
    /// <returns>The same <see cref="TestGroupBuilder"/> for call-chaining.</returns>
    public TestGroupBuilder AddTest(Action<TestProperties> configureProperties)
    {
        Assert.NotNull(configureProperties, nameof(configureProperties));

        var properties = new TestProperties();

        configureProperties(properties);

        _tests.Add(properties.ToTest());

        return this;
    }

    /// <summary>
    ///     Adds a collection of <see cref="ITest"/> implementations to the properties.
    /// </summary>
    /// <param name="tests">The tests to add.</param>
    /// <returns>The same <see cref="TestGroupBuilder"/> for call-chaining.</returns>
    public TestGroupBuilder AddTests(IEnumerable<ITest> tests)
    {
        Assert.NotNull(tests, nameof(tests));

        foreach (var test in tests)
            AddTest(test);

        return this;
    }

    /// <summary>
    ///     Adds a collection of <see cref="ITest"/> implementations to the properties.
    /// </summary>
    /// <param name="tests">The tests to add.</param>
    /// <returns>The same <see cref="TestGroupBuilder"/> for call-chaining.</returns>
    public TestGroupBuilder AddTests(params ITest[] tests)
    {
        Assert.NotNull(tests, nameof(tests));

        foreach (var test in tests)
            AddTest(test);

        return this;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ITestGroup"/> from the configured properties.
    /// </summary>
    /// <remarks>
    ///     This operation adds all defined <see cref="TestAttribute"/> on defined execution delegates of any command to the tests of the resulting instance.
    /// </remarks>
    /// <returns>A new instance of <see cref="ITestGroup"/>.</returns>
    public ITestGroup Build()
        => new TestGroup(_command, [.. _tests]);
}
