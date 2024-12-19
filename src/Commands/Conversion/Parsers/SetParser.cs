namespace Commands.Conversion
{
    internal sealed class SetParser<T>(TypeParser underlyingConverter) : TypeParser<T>, ICollectionConverter
    {
        public CollectionType CollectionType { get; } = CollectionType.Set;

        public override async ValueTask<ConvertResult> Parse(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (value is not object[] array)
                return Error($"The provided value is not an array. Expected: '{Type.Name}', got: '{value}'. At: '{argument.Name}'");

            var set = new HashSet<T>();

            foreach (var item in array)
            {
                var result = await underlyingConverter.Parse(consumer, argument, item, services, cancellationToken);

                if (!result.Success)
                    return Error($"Failed to convert an array element. Expected: '{underlyingConverter.Type.Name}', got: '{item}'. At: '{argument.Name}'");

                set.Add((T)result.Value!);
            }

            return Success(set);
        }
    }

    internal static class SetParser
    {
        private static readonly Dictionary<Type, TypeParser> _converters = [];

        public static TypeParser GetOrCreate(TypeParser underlyingConverter)
        {
            if (_converters.TryGetValue(underlyingConverter.Type, out var converter))
                return converter;

            converter = (TypeParser)Activator.CreateInstance(typeof(SetParser<>).MakeGenericType(underlyingConverter.Type), underlyingConverter)!;

            _converters.Add(underlyingConverter.Type, converter);

            return converter;
        }
    }
}
