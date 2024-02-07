using Commands.Core;
using Commands.Reflection;

namespace Commands.TypeConverters
{
    internal class EnumTypeReader(Type targetEnumType) : TypeConverterBase
    {
        private static readonly Dictionary<Type, EnumTypeReader> _readers = [];

        public override Type Type { get; } = targetEnumType;

        public override ValueTask<ConvertResult> EvaluateAsync(ICommandContext context, IServiceProvider services, IArgument parameter, string value, CancellationToken cancellationToken)
        {
            if (Enum.TryParse(Type, value, true, out var result))
                return ValueTask.FromResult(Success(result));

            return ValueTask.FromResult(Error($"The provided value is not a part the enum specified. Expected: '{Type.Name}', got: '{value}'. At: '{parameter.Name}'"));
        }

        internal static EnumTypeReader GetOrCreate(Type type)
        {
            if (_readers.TryGetValue(type, out var reader))
                return reader;

            _readers.Add(type, reader = new EnumTypeReader(type));

            return reader;
        }
    }
}
