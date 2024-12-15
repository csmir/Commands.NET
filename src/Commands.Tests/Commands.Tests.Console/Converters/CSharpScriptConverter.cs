using Commands.Converters;
using Commands.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Commands.Tests
{
    public class CSharpScriptConverter : TypeConverterBase<Delegate>
    {
        public override async ValueTask<ConvertResult> Evaluate(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
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
}
