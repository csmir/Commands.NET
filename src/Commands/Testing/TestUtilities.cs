using System;
using System.Collections.Generic;
using System.Text;

namespace Commands.Testing;

internal static class TestUtilities
{
    public static async ValueTask<TestResult> Test(this Command command, ICallerContext caller, ITestProvider provider, CommandOptions options)
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
            {
                argumentObjects[i] = parseResult[i].Value;
                continue;
            }    

            switch (parseResult[i].Exception)
            {
                case CommandOutOfRangeException:
                    if (provider.ExpectedResult == TestResultType.MatchFailure)
                        return TestResult.FromSuccess(provider.ExpectedResult);
                    else
                        return TestResult.FromError(provider.ExpectedResult, parseResult[i].Exception);
                default:
                    if (provider.ExpectedResult == TestResultType.ParseFailure)
                        return TestResult.FromSuccess(provider.ExpectedResult);
                    else
                        return TestResult.FromError(provider.ExpectedResult, parseResult[i].Exception);
            }
        }

        var runResult = await command.Run(caller, argumentObjects, options);

        if (runResult.Success)
        { 
            if (provider.ExpectedResult == TestResultType.Success)
                return TestResult.FromSuccess(provider.ExpectedResult);
            else
                return TestResult.FromError(provider.ExpectedResult, new InvalidOperationException("The command was expected to fail, but it succeeded."));
        }
        else
        {
            switch (runResult.Exception)
            {
                case PipelineConditionException:
                    if (provider.ExpectedResult == TestResultType.ConditionFailure)
                        return TestResult.FromSuccess(provider.ExpectedResult);
                    else
                        return TestResult.FromError(provider.ExpectedResult, runResult.Exception);

                default:
                    if (provider.ExpectedResult == TestResultType.InvocationFailure)
                        return TestResult.FromSuccess(provider.ExpectedResult);
                    else
                        return TestResult.FromError(provider.ExpectedResult, runResult.Exception);
            }
        }
    }
}
