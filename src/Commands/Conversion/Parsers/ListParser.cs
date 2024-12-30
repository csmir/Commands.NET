namespace Commands.Conversion
{
    internal sealed class ListParser<T>(TypeParser underlyingConverter) : TypeParser<T>, ICollectionParser
    {
        public CollectionType CollectionType { get; } = CollectionType.List;

        public override async ValueTask<ConvertResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (value is not object[] array)
                return Error($"The provided value is not an array. Expected: '{Type.Name}', got: '{value}'. At: '{argument.Name}'");

            var list = new List<T>();

            foreach (var item in array)
            {
                var result = await underlyingConverter.Parse(caller, argument, item, services, cancellationToken);

                if (!result.Success)
                    return Error($"Failed to convert an array element. Expected: '{underlyingConverter.Type.Name}', got: '{item}'. At: '{argument.Name}'");

                list.Add((T)result.Value!);
            }

            return Success(list);
        }
    }

    internal static class ListParser
    {
        private static readonly Dictionary<Type, TypeParser> _converters = [];

        public static TypeParser GetOrCreate(TypeParser underlyingConverter)
        {
            if (_converters.TryGetValue(underlyingConverter.Type, out var converter))
                return converter;

            converter = (TypeParser)Activator.CreateInstance(typeof(ListParser<>).MakeGenericType(underlyingConverter.Type), underlyingConverter)!;

            _converters.Add(underlyingConverter.Type, converter);

            return converter;
        }
    }
}
