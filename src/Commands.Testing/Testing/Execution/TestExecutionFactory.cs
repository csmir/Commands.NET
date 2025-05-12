namespace Commands.Testing;

/// <summary>
///     An implementation of <see cref="ITestExecutionFactory"/> which can be used to execute tests.
/// </summary>
public class TestExecutionFactory : ITestExecutionFactory
{
    /// <inheritdoc />
    public virtual async Task StartTesting<TContext>(Command command, Func<string, TContext> callerCreation, ExecutionOptions? options = null)
        where TContext : class, ICallerContext
    {
        options ??= ExecutionOptions.Default;

        Assert.NotNull(command, nameof(command));
        Assert.NotNull(callerCreation, nameof(callerCreation));

        if (options.ExecuteAsynchronously)
            throw new InvalidOperationException("The test execution cannot be run asynchronously. Please set the ExecuteAsynchronously option to false.");

        var context = CreateContext(command, options);

        if (context.Tests.Count == 0)
            return;

        var results = new TestResult[context.Tests.Count];

        for (var i = 0; i < context.Tests.Count; i++)
            results[i] = await command.TestUsing(callerCreation, context.Tests[i], options).ConfigureAwait(false);

        await HandleResult(context, results).ConfigureAwait(false);
    }

    /// <summary>
    ///     Creates a new <see cref="ITestContext"/> instance for the provided command and options.
    /// </summary>
    /// <remarks>
    ///     This method can be overridden to provide custom behavior for creating the test context.
    /// </remarks>
    /// <param name="command">The command that should be tested against.</param>
    /// <param name="options">The options to be used during test execution, determining pipeline settings.</param>
    /// <returns>A new implementation of <see cref="ITestContext"/> to be executed.</returns>
    protected virtual ITestContext CreateContext(Command command, ExecutionOptions options)
    {
        var tests = command.Attributes.OfType<ITest>().ToArray();

        var context = new TestContext(command)
        {
            CancellationSource = new CancellationTokenSource(),
            Tests = tests,
        };

        return context;
    }

    /// <summary>
    ///     Handles the result of the test execution.
    /// </summary>
    /// <remarks>
    ///     This method can be overridden to provide custom behavior for handling the result of the test execution.
    /// </remarks>
    /// <param name="context">The context that was used to execute the test pipeline.</param>
    /// <param name="results">An enumerable containing the result of every test present in the context when the pipeline was started.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the handling operation.</returns>
    protected virtual Task HandleResult(ITestContext context, IEnumerable<TestResult> results)
    {
        foreach (var result in results)
        {
            if (!result.Success)
                throw new InvalidOperationException($"Test {result.Test} for command {context.Command} failed: {result.Exception?.Message}");
        }

        return Task.CompletedTask;
    }
}
