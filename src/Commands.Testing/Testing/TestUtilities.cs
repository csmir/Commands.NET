namespace Commands.Testing;

internal static class TestUtilities
{
    public static async ValueTask<TestResult> TestUsing<TContext>(this Command command, Func<string, TContext> callerCreation, ITest test, ExecutionOptions options)
        where TContext : class, ICallerContext
    {
        TestResult GetResult(IResult result)
        {
            TestResult CompareReturn(TestResultType targetType, Exception exception)
            {
                return test.ShouldEvaluateTo == targetType
                    ? TestResult.FromSuccess(test, test.ShouldEvaluateTo)
                    : TestResult.FromError(test, test.ShouldEvaluateTo, targetType, exception);
            }

            return result.Exception switch
            {
                null => CompareReturn(TestResultType.Success, new InvalidOperationException("The command was expected to fail, but it succeeded.")),

                CommandParsingException => CompareReturn(TestResultType.ParseFailure, result.Exception),
                CommandEvaluationException => CompareReturn(TestResultType.ConditionFailure, result.Exception),
                CommandOutOfRangeException => CompareReturn(TestResultType.MatchFailure, result.Exception),

                _ => CompareReturn(TestResultType.InvocationFailure, result.Exception),
            };
        }

        var fullName = string.IsNullOrWhiteSpace(test.Arguments)
            ? command.GetFullName(false)
            : command.GetFullName(false) + ' ' + test.Arguments;

        var runResult = await command.Run(callerCreation(fullName), options).ConfigureAwait(false);

        return GetResult(runResult);
    }
}
