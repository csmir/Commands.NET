using Commands.Reflection;

namespace Commands.Converters
{
    internal sealed class ValueTypeConverter<T> : TypeConverterBase<T>
    {
        private delegate bool Parser<TValue>(string? str, out TValue value);

        private readonly static Lazy<IReadOnlyDictionary<Type, Delegate>> _container = new(ValueGenerator);

        public override async ValueTask<ConvertResult> Evaluate(
            CallerContext consumer, IArgument parameter, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var parser = (_container.Value[Type] as Parser<T>)!; // never null in cast use.

            if (parser(value?.ToString(), out var result))
                return Success(result);

            return Error($"The provided value does not match the expected type. Expected {typeof(T).Name}, got {value}. At: '{parameter.Name}'");
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

                // guid
                [typeof(Guid)] = (Parser<Guid>)Guid.TryParse
            };

            return callback;
        }
    }

    internal static class ValueTypeConverter
    {
        public static List<TypeConverterBase> CreateBaseConverters()
        {
            return
            [
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

                // guid
                new ValueTypeConverter<Guid>(),
            ];
        }
    }
}
