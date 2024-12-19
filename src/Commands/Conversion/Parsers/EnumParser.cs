namespace Commands.Conversion
{
    internal sealed class EnumParser(Type targetEnumType) : TypeParser
    {
        private static readonly Dictionary<Type, EnumParser> _converters = [];

        public override Type Type { get; } = targetEnumType;

        public override async ValueTask<ConvertResult> Parse(
            CallerContext consumer, IArgument parameter, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (Enum.TryParse(Type, value?.ToString(), true, out var result))
                return Success(result!);

            return Error($"The provided value is not a part the enum specified. Expected: '{Type.Name}', got: '{value}'. At: '{parameter.Name}'");
        }

        internal static EnumParser GetOrCreate(Type type)
        {
            if (_converters.TryGetValue(type, out var reader))
                return reader;

            _converters.Add(type, reader = new EnumParser(type));

            return reader;
        }
    }
}
