using Commands.Core;
using Commands.Reflection;

namespace Commands.TypeConverters
{
    internal sealed class ValueTypeConverter<T> : TypeConverterBase<T>
    {
        private delegate bool Parser<TValue>(string str, out TValue value);

        private readonly static Lazy<IReadOnlyDictionary<Type, Delegate>> _container = new(ValueGenerator);

        public override ValueTask<ConvertResult> EvaluateAsync(
            ConsumerBase consumer, IArgument parameter, string value, IServiceProvider services, CancellationToken cancellationToken)
        {
            var parser = _container.Value[Type] as Parser<T>;

            if (parser(value, out var result))
                return ValueTask.FromResult(Success(result));

            return ValueTask.FromResult(Error($"The provided value does not match the expected type. Expected {typeof(T).Name}, got {value}. At: '{parameter.Name}'"));
        }

        private static Dictionary<Type, Delegate> ValueGenerator()
        {
            var callback = new Dictionary<Type, Delegate>
            {
                // char
                [typeof(char)] = (Parser<char>)char.TryParse,

                // bit / boolean
                [typeof(bool)] = (Parser<bool>)bool.TryParse,

                // 8 bit int
                [typeof(byte)] = (Parser<byte>)byte.TryParse,
                [typeof(sbyte)] = (Parser<sbyte>)sbyte.TryParse,

                // 16 bit int
                [typeof(short)] = (Parser<short>)short.TryParse,
                [typeof(ushort)] = (Parser<ushort>)ushort.TryParse,

                // 32 bit int
                [typeof(int)] = (Parser<int>)int.TryParse,
                [typeof(uint)] = (Parser<uint>)uint.TryParse,

                // 64 bit int
                [typeof(long)] = (Parser<long>)long.TryParse,
                [typeof(ulong)] = (Parser<ulong>)ulong.TryParse,

                // floating point int
                [typeof(float)] = (Parser<float>)float.TryParse,
                [typeof(double)] = (Parser<double>)double.TryParse,
                [typeof(decimal)] = (Parser<decimal>)decimal.TryParse,

                // time
                [typeof(DateTime)] = (Parser<DateTime>)DateTime.TryParse,
                [typeof(DateTimeOffset)] = (Parser<DateTimeOffset>)DateTimeOffset.TryParse,
                [typeof(TimeOnly)] = (Parser<TimeOnly>)TimeOnly.TryParse,
                [typeof(DateOnly)] = (Parser<DateOnly>)DateOnly.TryParse,

                // guid
                [typeof(Guid)] = (Parser<Guid>)Guid.TryParse
            };

            return callback;
        }
    }

    internal static class ValueTypeConverter
    {
        public static TypeConverterBase[] CreateBaseConverters()
        {
            var callback = new TypeConverterBase[]
            {
                // char
                new ValueTypeConverter<char>(),

                // bit / boolean
                new ValueTypeConverter<bool>(),

                // 8 bit int
                new ValueTypeConverter<byte>(),
                new ValueTypeConverter<sbyte>(),

                // 16 bit int
                new ValueTypeConverter<short>(),
                new ValueTypeConverter<ushort>(),

                // 32 bit int
                new ValueTypeConverter<int>(),
                new ValueTypeConverter<uint>(),

                // 64 bit int
                new ValueTypeConverter<long>(),
                new ValueTypeConverter<ulong>(),

                // floating point int
                new ValueTypeConverter<float>(),
                new ValueTypeConverter<double>(),
                new ValueTypeConverter<decimal>(),

                // time
                new ValueTypeConverter<DateTime>(),
                new ValueTypeConverter<DateTimeOffset>(),
                new ValueTypeConverter<TimeOnly>(),
                new ValueTypeConverter<DateOnly>(),

                // guid
                new ValueTypeConverter<Guid>(),
            };

            return callback;
        }
    }
}
