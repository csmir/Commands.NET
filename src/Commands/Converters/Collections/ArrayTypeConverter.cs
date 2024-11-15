using Commands.Reflection;

namespace Commands.Converters
{
    internal sealed class ArrayTypeConverter<T>(TypeConverterBase underlyingConverter) : TypeConverterBase<T>
    {
        public override async ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (value is not object[] array)
                return Error($"The provided value is not an array. Expected: '{Type.Name}', got: '{value}'. At: '{argument.Name}'");

            // Create an instance of the array type.
            var instance = new T[array.Length];

            for (var i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);

                var result = await underlyingConverter.Evaluate(consumer, argument, item, services, cancellationToken);

                if (!result.Success)
                    return Error($"Failed to convert an array element. Expected: '{underlyingConverter.Type.Name}', got: '{item}'. At: '{argument.Name}', Index: '{i}'");

                instance[i] = (T)result.Value!;
            }

            return Success(instance);
        }
    }

    internal static class ArrayTypeConverter
    {
        private static readonly Dictionary<Type, TypeConverterBase> _converters = [];

        public static TypeConverterBase GetOrCreate(TypeConverterBase underlyingConverter)
        {
            if (_converters.TryGetValue(underlyingConverter.Type, out var converter))
                return converter;

            converter = (TypeConverterBase)Activator.CreateInstance(typeof(ArrayTypeConverter<>).MakeGenericType(underlyingConverter.Type), underlyingConverter)!;

            _converters.Add(underlyingConverter.Type, converter);

            return converter;
        }
    }
}
