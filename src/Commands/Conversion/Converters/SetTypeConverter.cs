namespace Commands.Conversion
{
    internal sealed class SetTypeConverter<T>(TypeConverter underlyingConverter) : TypeConverter<T>, ICollectionConverter
    {
        public CollectionType CollectionType { get; } = CollectionType.Set;

        public override async ValueTask<ConvertResult> Evaluate(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
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
        private static readonly Dictionary<Type, TypeConverter> _converters = [];

        public static TypeConverter GetOrCreate(TypeConverter underlyingConverter)
        {
            if (_converters.TryGetValue(underlyingConverter.Type, out var converter))
                return converter;

            converter = (TypeConverter)Activator.CreateInstance(typeof(SetTypeConverter<>).MakeGenericType(underlyingConverter.Type), underlyingConverter)!;

            _converters.Add(underlyingConverter.Type, converter);

            return converter;
        }
    }
}
