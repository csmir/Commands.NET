namespace Commands.Testing;

/// <summary>
///     
/// </summary>
public interface ITestProvider : ITestCollection<TestGroup>
{
    /// <summary>
    ///     
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="callerCreation"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public ValueTask<IEnumerable<TestResult>> Execute<TContext>(Func<string, TContext> callerCreation, CommandOptions? options = null)
        where TContext : class, ICallerContext;
}
