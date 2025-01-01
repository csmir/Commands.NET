// Documentation of this file can be found at https://github.com/csmir/Commands.NET/wiki/Type-Conversion.

using Commands.Conversion;

namespace Commands.Samples
{
    public class LiteralTypeParser(bool caseIgnore) : TypeParser<Type>
    {
        private readonly bool _caseIgnore = caseIgnore;

        public override ValueTask<ParseResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            try
            {
                var typeSrc = Type.GetType(
                    typeName: value?.ToString() ?? "",
                    throwOnError: true,
                    ignoreCase: _caseIgnore);

                if (typeSrc == null)
                {
                    return Error($"A type with name '{value}' was not found.");
                }

                return Success(typeSrc);
            }
            catch (Exception ex)
            {
                return Error(ex);
            }
        }
    }
}
