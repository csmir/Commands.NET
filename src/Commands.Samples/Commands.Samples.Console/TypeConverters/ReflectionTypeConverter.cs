// Documentation of this file can be found at https://github.com/csmir/Commands.NET/wiki/Type-Conversion.

using Commands.Core;
using Commands.Reflection;
using Commands.TypeConverters;

namespace Commands.Samples
{
    public class ReflectionTypeConverter(bool caseIgnore) : TypeConverterBase<Type>
    {
        private readonly bool _caseIgnore = caseIgnore;

        public override ValueTask<ConvertResult> EvaluateAsync(ConsumerBase consumer, IArgument argument, string value, IServiceProvider services, CancellationToken cancellationToken)
        {
            try
            {
                var typeSrc = Type.GetType(
                    typeName: value,
                    throwOnError: true,
                    ignoreCase: _caseIgnore);

                if (typeSrc == null)
                {
                    return ValueTask.FromResult(Error($"A type with name '{value}' was not found."));
                }

                return ValueTask.FromResult(Success(typeSrc));
            }
            catch (Exception ex)
            {
                return ValueTask.FromResult(Error(ex));
            }
        }
    }
}
