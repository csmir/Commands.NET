using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Commands.Tests;

public class CSharpScriptParserAttribute : TypeParserAttribute<Delegate>
{
    public override ValueTask<ParseResult> Parse(ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        => new CSharpScriptParser().Parse(caller, argument, value, services, cancellationToken);
}

public class CSharpScriptParser : TypeParser<Delegate>
{
    public override async ValueTask<ParseResult> Parse(ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (value is string script)
        {
            var scriptOptions = ScriptOptions.Default
                .WithReferences(typeof(Program).Assembly);

            try
            {
                var executionAction = await CSharpScript.EvaluateAsync<Delegate>(script, scriptOptions, cancellationToken: cancellationToken);

                return Success(executionAction);
            }
            catch (CompilationErrorException ex)
            {
                return Error(ex.Message);
            }
        }

        return Error("The provided value is not a valid C# script.");
    }
}
