namespace Commands.Conversion
{
    internal sealed class ArrayParser(TypeParser underlyingConverter) : TypeParser
    {
        private static readonly Dictionary<Type, TypeParser> _converters = [];

        public override Type Type => underlyingConverter.Type;

        public override async ValueTask<ConvertResult> Parse(ICallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (value is not object[] array)
                return Error($"The provided value is not an array. Expected: '{Type.Name}', got: '{value}'. At: '{argument.Name}'");

            var instance = Array.CreateInstance(Type, array.Length);

            for (var i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);

                var result = await underlyingConverter.Parse(consumer, argument, item, services, cancellationToken);

                if (!result.Success)
                    return Error($"Failed to convert an array element. Expected: '{underlyingConverter.Type.Name}', got: '{item}'. At: '{argument.Name}', Index: '{i}'");

                instance.SetValue(result.Value, i);
            }

            return Success(instance);
        }

        public static TypeParser GetOrCreate(TypeParser underlyingConverter)
        {
            if (_converters.TryGetValue(underlyingConverter.Type, out var converter))
                return converter;

            converter = new ArrayParser(underlyingConverter)!;

            _converters.Add(underlyingConverter.Type, converter);

            return converter;
        }
    }
}
