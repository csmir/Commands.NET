using Commands.Reflection;

namespace Commands.Converters
{
    internal sealed class EnumTypeReader(Type targetEnumType) : TypeConverterBase
    {
        private static readonly Dictionary<Type, EnumTypeReader> _converters = [];

        public override Type Type { get; } = targetEnumType;

        public override ValueTask<ConvertResult> Evaluate(
            ConsumerBase consumer, IArgument parameter, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (Enum.TryParse(Type, value?.ToString(), true, out var result))
                return ValueTask.FromResult(Success(result!));

            return ValueTask.FromResult(Error($"The provided value is not a part the enum specified. Expected: '{Type.Name}', got: '{value}'. At: '{parameter.Name}'"));
        }

        internal static EnumTypeReader GetOrCreate(Type type)
        {
            if (_converters.TryGetValue(type, out var reader))
                return reader;

            _converters.Add(type, reader = new EnumTypeReader(type));

            return reader;
        }
    }
}
