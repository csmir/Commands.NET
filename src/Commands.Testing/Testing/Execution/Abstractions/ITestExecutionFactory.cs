namespace Commands.Testing;

/// <summary>
///     
/// </summary>
public interface ITestExecutionFactory
{
    /// <summary>
    ///     Sequentially tests all available <see cref="ITest"/> instances against the <paramref name="command"/> using the provided <paramref name="callerCreation"/> and options.
    /// </summary>
    /// <remarks>
    ///     When specifying the <paramref name="options"/> of this operation, the value of <see cref="CommandOptions.ExecuteAsynchronously"/> is ignored. 
    ///     This is because the test execution expects to yield results directly back to the executing thread, and cannot do this in detached context.
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="ICallerContext"/> that this test sequence should use to test with.</typeparam>
    /// <param name="command">The command to target for querying available tests, and test execution.</param>
    /// <param name="callerCreation">A delegate that yields an implementation of <typeparamref name="TContext"/> based on the input value for every new test.</param>
    /// <param name="options">A collection of options that determine how every test against this command is ran.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> containing an <see cref="IEnumerable{T}"/> with the result of every test yielded by this operation.</returns>
    public Task StartTesting<TContext>(Command command, Func<string, TContext> callerCreation, CommandOptions? options = null)
        where TContext : class, ICallerContext;
}
