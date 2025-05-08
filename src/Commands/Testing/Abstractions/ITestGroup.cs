namespace Commands.Testing;

/// <summary>
///     Defines a non-unique mutable <see cref="IGrouping{TKey, TElement}"/> structured collection, where the key is <see cref="Command"/> and the values are the tests targetted at that command.
/// </summary>
public interface ITestGroup : ICollection<ITest>, IEnumerable<ITest>
{
    /// <summary>
    ///     Gets the command which the tests contained in this <see cref="ITestGroup"/> should be tested against.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    ///     Sequentially tests all available <see cref="ITest"/> instances aaginst the <see cref="Command"/> contained within this type using the provided <paramref name="callerCreation"/> and options.
    /// </summary>
    /// <remarks>
    ///     When specifying the <paramref name="options"/> of this operation, the value of <see cref="CommandOptions.ExecuteAsynchronously"/> is ignored. 
    ///     This is because the test execution expects to yield results directly back to the caller, and cannot do this in detached context.
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="ICallerContext"/> that this test sequence should use to test with.</typeparam>
    /// <param name="callerCreation">A delegate that yields an implementation of <typeparamref name="TContext"/> based on the input value for every new test.</param>
    /// <param name="options">A collection of options that determine how every test against this command is ran.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> containing an <see cref="IEnumerable{T}"/> with the result of every test yielded by this operation.</returns>
    public ValueTask<IEnumerable<TestResult>> Run<TContext>(Func<string, TContext> callerCreation, CommandOptions? options = null)
        where TContext : class, ICallerContext;
}
