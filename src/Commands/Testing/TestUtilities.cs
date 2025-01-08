namespace Commands.Testing;

internal static class TestUtilities
{
    internal static async ValueTask<TestResult> Test(this Command command, ICallerContext caller, ITestProvider provider, CommandOptions options)
    {
        // Create a new set of arguments by appending the arguments from the provider to the command's name. This should result in valid parameters for an ArgumentArray.
        var commandName = command.GetFullName(false)
            .Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        var fullName = string.IsNullOrWhiteSpace(provider.Arguments)
            ? string.Join(" ", commandName)
            : string.Join(" ", commandName) + ' ' + provider.Arguments;

        var parseIndex = commandName.Length;

        var arguments = ArgumentArray.Read(fullName);

        var parseResult = await command.Parse(caller, parseIndex, arguments, options);

        var argumentObjects = new object?[parseResult.Length];

        for (var i = 0; i < parseResult.Length; i++)
        {
            if (parseResult[i].Success)
                argumentObjects[i] = parseResult[i].Value;
            else
                return provider.GetResult(command, parseResult[i]);
        }

        var runResult = await command.Run(caller, argumentObjects, options);

        return provider.GetResult(command, runResult);
    }

    internal static TestResult GetResult(this ITestProvider provider, Command command, IExecuteResult result)
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

            ParserException            => CompareReturn(TestResultType.ParseFailure, result.Exception),
            CommandEvaluationException => CompareReturn(TestResultType.ConditionFailure, result.Exception),
            CommandOutOfRangeException => CompareReturn(TestResultType.MatchFailure, result.Exception),

            _ => CompareReturn(TestResultType.InvocationFailure, result.Exception),
        };
    }
}
