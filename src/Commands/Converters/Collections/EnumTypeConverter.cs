using Commands.Reflection;

namespace Commands.Converters
{
    internal sealed class EnumTypeReader(Type targetEnumType) : TypeConverterBase
    {
        private static readonly Dictionary<Type, EnumTypeReader> _converters = [];

        public override Type Type { get; } = targetEnumType;

        public override async ValueTask<ConvertResult> Evaluate(
            CallerContext consumer, IArgument parameter, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (Enum.TryParse(Type, value?.ToString(), true, out var result))
                return Success(result!);

            return Error($"The provided value is not a part the enum specified. Expected: '{Type.Name}', got: '{value}'. At: '{parameter.Name}'");
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
