namespace Commands.Testing;

internal static class TestUtilities
{
    internal static async ValueTask<TestResult> Test(this Command command, ICallerContext caller, ITestProvider provider, CommandOptions options)
    {
        // Create a new set of arguments by appending the arguments from the provider to the command's name. This should result in valid parameters for an ArgumentArray.
        var commandName = command.GetFullName(false)
            .Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new KeyValuePair<string, object?>(x, null))
            .ToArray();

        var parseIndex = commandName.Length;

        var arguments = new ArgumentArray([.. commandName, ..provider.Arguments], StringComparer.OrdinalIgnoreCase);

        var parseResult = await command.Parse(caller, parseIndex, arguments, options);

        var argumentObjects = new object?[parseResult.Length];

        for (var i = 0; i < parseResult.Length; i++)
        {
            if (parseResult[i].Success)
                argumentObjects[i] = parseResult[i].Value;
            else
                return provider.GetResult(parseResult[i]);
        }

        var runResult = await command.Run(caller, argumentObjects, options);

        return provider.GetResult(runResult);
    }

    internal static TestResult GetResult(this ITestProvider provider, IExecuteResult result)
    {
        TestResult CompareReturn(TestResultType targetType, Exception exception)
        {
            return provider.ExpectedResult == targetType
                ? TestResult.FromSuccess(provider.ExpectedResult)
                : TestResult.FromError(targetType, exception);
        }

        return result.Exception switch
        {
            null => CompareReturn(TestResultType.Success, new InvalidOperationException("The command was expected to fail, but it succeeded.")),

            CommandParsingException    => CompareReturn(TestResultType.ParseFailure, result.Exception),
            CommandEvaluationException => CompareReturn(TestResultType.ConditionFailure, result.Exception),
            CommandOutOfRangeException => CompareReturn(TestResultType.MatchFailure, result.Exception),

            _ => CompareReturn(TestResultType.InvocationFailure, result.Exception),
        };
    }
}
