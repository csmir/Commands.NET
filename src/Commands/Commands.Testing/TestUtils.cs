using System.ComponentModel;

namespace Commands.Testing;

/// <summary>
///     A static class containing methods for testing commands.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TestUtils
{
    /// <summary>
    ///     Tests the target command using the provided <paramref name="contextCreationDelegate"/> function to create the context for each individual execution.
    /// </summary>
    /// <remarks>
    ///     Define tests on commands using the <see cref="TestAttribute"/> attribute on the command method or delegate.
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="IContext"/> that this test sequence should use to test with.</typeparam>
    /// <param name="command">The command to target for querying available tests, and test execution.</param>
    /// <param name="contextCreationDelegate">A delegate that yields an implementation of <typeparamref name="TContext"/> based on the input value for every new test.</param>
    /// <param name="options">A collection of options that determine how every test against this command is ran.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> containing an <see cref="IEnumerable{T}"/> with the result of every test yielded by this operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> or <paramref name="contextCreationDelegate"/> is <see langword="null"/>.</exception>
    public static async ValueTask<IEnumerable<TestResult>> Test<TContext>(this Command command, Func<string, TContext> contextCreationDelegate, ExecutionOptions? options = null)
        where TContext : class, IContext
    {
        static TestResult Compare(ITest test, TestResultType targetType, Exception exception)
        {
            return test.ShouldEvaluateTo == targetType
                ? TestResult.FromSuccess(test, test.ShouldEvaluateTo)
                : TestResult.FromError(test, test.ShouldEvaluateTo, targetType, exception);
        }

        Assert.NotNull(command, nameof(command));
        Assert.NotNull(contextCreationDelegate, nameof(contextCreationDelegate));

        options ??= ExecutionOptions.Default;

        if (options.ExecuteAsynchronously)
            options.ExecuteAsynchronously = false;

        var tests = command.Attributes.OfType<ITest>().ToArray();

        var results = new TestResult[tests.Length];

        for (var i = 0; i < tests.Length; i++)
        {
            var test = tests[i];

            var fullName = string.IsNullOrWhiteSpace(test.Arguments)
                ? command.GetFullName(false)
                : command.GetFullName(false) + ' ' + test.Arguments;

            var runResult = await command.Run(contextCreationDelegate(fullName), options).ConfigureAwait(false);

            results[i] = runResult.Exception switch
            {
                null => Compare(test, TestResultType.Success, new InvalidOperationException("The command was expected to fail, but it succeeded.")),

                CommandParsingException => Compare(test, TestResultType.ParseFailure, runResult.Exception),
                CommandEvaluationException => Compare(test, TestResultType.ConditionFailure, runResult.Exception),
                CommandOutOfRangeException => Compare(test, TestResultType.MatchFailure, runResult.Exception),

                _ => Compare(test, TestResultType.InvocationFailure, runResult.Exception),
            };
        }

        return results;
    }
}
