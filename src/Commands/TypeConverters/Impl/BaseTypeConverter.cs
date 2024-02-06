using Commands.Core;
using Commands.Reflection;

namespace Commands.TypeConverters
{
    internal class BaseTypeConverter<T> : TypeConverter<T>
    {
        private delegate bool Converter<TValue>(string str, out TValue value);

        private readonly static Lazy<IReadOnlyDictionary<Type, Delegate>> _container = new(ValueGenerator);

        public override ValueTask<ConvertResult> EvaluateAsync(ICommandContext context, IServiceProvider services, IArgument parameter, string value, CancellationToken cancellationToken)
        {
            var parser = _container.Value[Type] as Converter<T>;

            if (parser(value, out var result))
                return ValueTask.FromResult(Success(result));

            return ValueTask.FromResult(Error($"The provided value does not match the expected type. Expected {typeof(T).Name}, got {value}. At: '{parameter.Name}'"));
        }

        private static Dictionary<Type, Delegate> ValueGenerator()
        {
            var callback = new Dictionary<Type, Delegate>
            {
                // char
                [typeof(char)] = (Converter<char>)char.TryParse,

                // bit / boolean
                [typeof(bool)] = (Converter<bool>)bool.TryParse,

                // 8 bit int
                [typeof(byte)] = (Converter<byte>)byte.TryParse,
                [typeof(sbyte)] = (Converter<sbyte>)sbyte.TryParse,

                // 16 bit int
                [typeof(short)] = (Converter<short>)short.TryParse,
                [typeof(ushort)] = (Converter<ushort>)ushort.TryParse,

                // 32 bit int
                [typeof(int)] = (Converter<int>)int.TryParse,
                [typeof(uint)] = (Converter<uint>)uint.TryParse,

                // 64 bit int
                [typeof(long)] = (Converter<long>)long.TryParse,
                [typeof(ulong)] = (Converter<ulong>)ulong.TryParse,

                // floating point int
                [typeof(float)] = (Converter<float>)float.TryParse,
                [typeof(double)] = (Converter<double>)double.TryParse,
                [typeof(decimal)] = (Converter<decimal>)decimal.TryParse,

                // time
                [typeof(DateTime)] = (Converter<DateTime>)DateTime.TryParse,
                [typeof(DateTimeOffset)] = (Converter<DateTimeOffset>)DateTimeOffset.TryParse,
                [typeof(TimeOnly)] = (Converter<TimeOnly>)TimeOnly.TryParse,
                [typeof(DateOnly)] = (Converter<DateOnly>)DateOnly.TryParse,

                // guid
                [typeof(Guid)] = (Converter<Guid>)Guid.TryParse
            };

            return callback;
        }
    }

    internal static class BaseTypeConverter
    {
        public static TypeConverter[] CreateBaseConverters()
        {
            var callback = new TypeConverter[]
            {
                // char
                new BaseTypeConverter<char>(),

                // bit / boolean
                new BaseTypeConverter<bool>(),

                // 8 bit int
                new BaseTypeConverter<byte>(),
                new BaseTypeConverter<sbyte>(),

                // 16 bit int
                new BaseTypeConverter<short>(),
                new BaseTypeConverter<ushort>(),

                // 32 bit int
                new BaseTypeConverter<int>(),
                new BaseTypeConverter<uint>(),

                // 64 bit int
                new BaseTypeConverter<long>(),
                new BaseTypeConverter<ulong>(),

                // floating point int
                new BaseTypeConverter<float>(),
                new BaseTypeConverter<double>(),
                new BaseTypeConverter<decimal>(),

                // time
                new BaseTypeConverter<DateTime>(),
                new BaseTypeConverter<DateTimeOffset>(),
                new BaseTypeConverter<TimeOnly>(),
                new BaseTypeConverter<DateOnly>(),

                // guid
                new BaseTypeConverter<Guid>(),
            };

            return callback;
        }
    }
}
