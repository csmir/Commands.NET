namespace Commands.Testing;

/// <summary>
///     
/// </summary>
public sealed class TestCollection : ITestProvider
{
    private TestGroup[] _tests;

    /// <summary>
    ///     Gets the number of groups contained in this collection.
    /// </summary>
    public int Count
        => _tests.Length;

    /// <summary>
    ///     
    /// </summary>
    /// <param name="tests"></param>
    public TestCollection(params TestGroup[] tests)
    {
        _tests = tests;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="callerCreation"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public async ValueTask<IEnumerable<TestResult>> Execute<TContext>(Func<string, TContext> callerCreation, CommandOptions? options = null)
        where TContext : class, ICallerContext
    {
        options ??= new CommandOptions();

        var results = new IEnumerable<TestResult>[_tests.Length];

        for (var i = 0; i < _tests.Length; i++)
            results[i] = await _tests[i].Run(callerCreation, options).ConfigureAwait(false);

        return results.SelectMany(x => x);
    }

    /// <inheritdoc />
    public void Add(TestGroup item)
    {
        Assert.NotNull(item, nameof(item));

        Array.Resize(ref _tests, _tests.Length);

        _tests[_tests.Length] = item;
    }

    /// <inheritdoc />
    public void Clear()
        => _tests = [];

    /// <inheritdoc />
    public bool Contains(TestGroup item)
    {
        Assert.NotNull(item, nameof(item));

        return _tests.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(TestGroup[] array, int arrayIndex)
        => _tests.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(TestGroup item)
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
    public IEnumerator<TestGroup> GetEnumerator()
        => ((IEnumerable<TestGroup>)_tests).GetEnumerator();

    bool ICollection<TestGroup>.IsReadOnly
        => false;

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
