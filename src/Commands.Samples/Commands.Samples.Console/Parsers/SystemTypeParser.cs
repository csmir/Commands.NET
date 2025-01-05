using Commands.Conversion;

namespace Commands.Samples;

// A parser that converts a string to a System.Type found within the current assembly.
public class SystemTypeParser(bool caseIgnore) : TypeParser<Type>
{
    private readonly bool _caseIgnore = caseIgnore;

    public override ValueTask<ParseResult> Parse(ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        try
        {
            var typeSrc = Type.GetType(
                typeName: value?.ToString() ?? "",
                throwOnError: true,
                ignoreCase: _caseIgnore);

            return Success(typeSrc);
        }
        catch
        {
            return Error($"A type with name '{value}' was not found within the current assembly. Did you provide the type's full name, including its namespace?");
        }
    }
}
