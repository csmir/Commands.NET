namespace Commands.Testing;

internal static class TestUtilities
{
    public static async ValueTask<TestResult> Test(this Command command, Func<string, ICallerContext> callerCreation, ITestProvider provider, CommandOptions options)
    {
        TestResult GetResult(IResult result)
        {
            TestResult CompareReturn(TestResultType targetType, Exception exception)
            {
                return provider.ShouldEvaluateTo == targetType
                    ? TestResult.FromSuccess(command, provider.ShouldEvaluateTo)
                    : TestResult.FromError(command, provider.ShouldEvaluateTo, targetType, exception);
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

        var fullName = string.IsNullOrWhiteSpace(provider.Arguments)
            ? command.GetFullName(false)
            : command.GetFullName(false) + ' ' + provider.Arguments;

        var runResult = await command.Run(callerCreation(fullName), options).ConfigureAwait(false);

        return GetResult(runResult);
    }
}
