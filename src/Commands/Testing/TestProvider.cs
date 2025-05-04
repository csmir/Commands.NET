namespace Commands.Testing;

/// <summary>
///     Represents a provider containing <see cref="ITest"/> implementations to be tested against the <see cref="Command"/> that this group targets.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class TestProvider : ITestProvider
{
    private ITest[] _tests;

    /// <inheritdoc />
    public Command Command { get; }

    /// <summary>
    ///     Gets the number of <see cref="ITest"/> implementations contained in this group.
    /// </summary>
    public int Count
        => _tests.Length;

    /// <summary>
    ///     Creates a new <see cref="TestProvider"/> targetting the provided command, including the provided tests.
    /// </summary>
    /// <remarks>
    ///     This constructor adds all implementations of <see cref="TestAttribute"/> marked on defined execution delegates, alongside the provided <paramref name="tests"/>.
    /// </remarks>
    /// <param name="command">The command that should be tested against.</param>
    /// <param name="tests">A variable collection of tests this command should be tested with.</param>
    public TestProvider(Command command, params ITest[] tests)
    {
        Assert.NotNull(command, nameof(command));
        Assert.NotNull(tests, nameof(tests));

        Command = command;

        _tests = [.. tests, .. command.Attributes.OfType<ITest>()];
    }

    /// <inheritdoc />
    public async ValueTask<IEnumerable<TestResult>> Test<TContext>(Func<string, TContext> callerCreation, CommandOptions? options = null)
        where TContext : class, ICallerContext
    {
        Assert.NotNull(callerCreation, nameof(callerCreation));

        // Permit a fast path for empty providers.
        if (_tests.Length == 0)
            return [];

        options ??= new CommandOptions();

        var results = new TestResult[_tests.Length];

        for (var i = 0; i < _tests.Length; i++)
            results[i] = await Command.TestUsing(callerCreation, _tests[i], options).ConfigureAwait(false);

        return results;
    }

    /// <inheritdoc />
    public void Add(ITest item)
    {
        Assert.NotNull(item, nameof(item));

        Array.Resize(ref _tests, _tests.Length + 1);

        _tests[_tests.Length] = item;
    }

    /// <inheritdoc />
    public void Clear()
        => _tests = [];

    /// <inheritdoc />
    public bool Contains(ITest item)
    {
        Assert.NotNull(item, nameof(item));

        return _tests.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(ITest[] array, int arrayIndex)
        => _tests.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(ITest item)
    {
        Assert.NotNull(item, nameof(item));

        var indexOf = Array.IndexOf(_tests, item);

        if (indexOf == -1)
            return false;

        for (var i = indexOf; i < _tests.Length - 1; i++)
            _tests[i] = _tests[i + 1];

        Array.Resize(ref _tests, _tests.Length - 1);

        return true;
    }

    /// <inheritdoc />
    public IEnumerator<ITest> GetEnumerator()
        => ((IEnumerable<ITest>)_tests).GetEnumerator();

    /// <inheritdoc />
    public override string ToString()
        => $"Count = {Count}\nCommand = {Command}";

    #region Internals

    bool ICollection<ITest>.IsReadOnly
        => false;

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    #endregion

    #region Initializers

    /// <summary>
    ///     Defines a collection of properties from the provided command, to configure and construct a new instance of <see cref="TestProvider"/> from.
    /// </summary>
    /// <param name="command">The command to be represented by the constructed type, being a testable interface to evaluate execution for.</param>
    /// <returns>A new instance of <see cref="TestProviderProperties"/> to configure and construct into a new instance of <see cref="TestProvider"/>.</returns>
    public static TestProviderProperties From(Command command)
        => new TestProviderProperties().WithCommand(command);

    #endregion
}
