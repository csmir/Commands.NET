namespace Commands.Testing;

/// <summary>
///     Represents a group of <see cref="ITest"/> implementations to be tested against the <see cref="Command"/> that this group targets.
/// </summary>
public sealed class TestGroup : ITestCollection<ITest>
{
    private ITest[] _tests;

    /// <summary>
    ///     Gets the command which the tests contained in this <see cref="TestGroup"/> should be tested against.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    ///     Gets the number of <see cref="ITest"/> implementations contained in this group.
    /// </summary>
    public int Count 
        => _tests.Length;

    /// <summary>
    ///     Creates a new <see cref="TestGroup"/> targetting the provided command, including the provided tests.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="tests"></param>
    public TestGroup(Command command, params ITest[] tests)
    {
        Assert.NotNull(command, nameof(command));
        Assert.NotNull(tests, nameof(tests));

        Command = command;

        _tests = tests;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="callerCreation"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public async ValueTask<IEnumerable<TestResult>> Run<TContext>(Func<string, TContext> callerCreation, CommandOptions options)
        where TContext : class, ICallerContext
    {
        Assert.NotNull(callerCreation, nameof(callerCreation));
        Assert.NotNull(options, nameof(options));

        var results = new TestResult[_tests.Length];

        for (var i = 0; i < _tests.Length; i++)
            results[i] = await Command.TestAgainst(callerCreation, _tests[i], options).ConfigureAwait(false);

        return results;
    }

    /// <inheritdoc />
    public void Add(ITest item)
    {
        Assert.NotNull(item, nameof(item));

        Array.Resize(ref _tests, _tests.Length);

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

    bool ICollection<ITest>.IsReadOnly
        => false;

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
