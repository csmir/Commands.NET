using Commands.Parsing;

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

                CommandParsingException    => CompareReturn(TestResultType.ParseFailure, result.Exception),
                CommandEvaluationException => CompareReturn(TestResultType.ConditionFailure, result.Exception),
                CommandOutOfRangeException => CompareReturn(TestResultType.MatchFailure, result.Exception),

                _ => CompareReturn(TestResultType.InvocationFailure, result.Exception),
            };
        }

        // Create a new set of arguments by appending the arguments from the provider to the command's name. This should result in valid parameters for an ArgumentArray.
        var commandName = command.GetFullName(false)
            .Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .ToArray();

        var fullName = string.IsNullOrWhiteSpace(provider.Arguments)
            ? string.Join(" ", commandName)
            : string.Join(" ", commandName) + ' ' + provider.Arguments;

        var parseIndex = commandName.Length;

        var arguments = ArgumentArray.Read(fullName);

        var parseResult = await command.Parse(caller, parseIndex, arguments, options).ConfigureAwait(false);

        var argumentObjects = new object?[parseResult.Length];

        for (var i = 0; i < parseResult.Length; i++)
        {
            if (parseResult[i].Success)
                argumentObjects[i] = parseResult[i].Value;
            else
                return GetResult(ParseResult.FromError(new CommandParsingException(command, parseResult[i].Exception)));
        }

        var runResult = await command.Run(caller, argumentObjects, options).ConfigureAwait(false);

        return GetResult(runResult);
    }
}
