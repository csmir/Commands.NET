namespace Commands.Testing;

public class TestExecutionFactory : ITestExecutionFactory
{
    /// <inheritdoc />
    public virtual async Task StartTesting<TContext>(Command command, Func<string, TContext> callerCreation, CommandOptions? options = null)
        where TContext : class, ICallerContext
    {
        options ??= CommandOptions.Default;

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
    
    protected virtual ITestContext CreateContext(Command command, CommandOptions options)
    {
        var tests = command.Attributes.OfType<ITest>().ToArray();

        var context = new TestContext(command)
        {
            CancellationSource = new CancellationTokenSource(),
            Tests = tests,
        };

        return context;
    }

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
