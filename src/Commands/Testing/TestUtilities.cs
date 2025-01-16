namespace Commands.Testing;

internal static class TestUtilities
{
    internal static async ValueTask<TestResult> Test(this Command command, ICallerContext caller, ITestProvider provider, CommandOptions options)
    {
        TestResult GetResult(IExecuteResult result)
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

        var arguments = ArgumentArray.Read(fullName);

        var runResult = await command.Run(caller, arguments, options).ConfigureAwait(false);

        return GetResult(runResult);
    }
}
