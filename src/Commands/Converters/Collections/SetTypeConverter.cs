using Commands.Reflection;

namespace Commands.Converters
{
    internal sealed class SetTypeConverter<T>(TypeConverterBase underlyingConverter) : TypeConverterBase<T>
    {
        public override async ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (value is not object[] array)
                return Error($"The provided value is not an array. Expected: '{Type.Name}', got: '{value}'. At: '{argument.Name}'");

            var set = new HashSet<T>();

            foreach (var item in array)
            {
                var result = await underlyingConverter.Evaluate(consumer, argument, item, services, cancellationToken);

                if (!result.Success)
                    return Error($"Failed to convert an array element. Expected: '{underlyingConverter.Type.Name}', got: '{item}'. At: '{argument.Name}'");

                set.Add((T)result.Value!);
            }

            return Success(set);
        }
    }

    internal static class SetTypeConverter
    {
        private static readonly Dictionary<Type, TypeConverterBase> _converters = [];

        public static TypeConverterBase GetOrCreate(TypeConverterBase underlyingConverter)
        {
            if (_converters.TryGetValue(underlyingConverter.Type, out var converter))
                return converter;

            converter = (TypeConverterBase)Activator.CreateInstance(typeof(SetTypeConverter<>).MakeGenericType(underlyingConverter.Type), underlyingConverter)!;

            _converters.Add(underlyingConverter.Type, converter);

            return converter;
        }
    }
}
