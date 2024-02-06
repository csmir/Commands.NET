using Commands.Core;
using Commands.Reflection;

namespace Commands.TypeConverters
{
    internal class BaseTypeReader<T> : TypeConverter<T>
    {
        private delegate bool Tpd<TValue>(string str, out TValue value);

        private readonly static Lazy<IReadOnlyDictionary<Type, Delegate>> _container = new(ValueGenerator);

        public override ValueTask<ConvertResult> EvaluateAsync(ICommandContext context, IServiceProvider services, IArgument parameter, string value, CancellationToken cancellationToken)
        {
            var parser = _container.Value[Type] as Tpd<T>;

            if (parser(value, out var result))
                return ValueTask.FromResult(Success(result));

            return ValueTask.FromResult(Error($"The provided value does not match the expected type. Expected {typeof(T).Name}, got {value}. At: '{parameter.Name}'"));
        }

        private static Dictionary<Type, Delegate> ValueGenerator()
        {
            var callback = new Dictionary<Type, Delegate>
            {
                // char
                [typeof(char)] = (Tpd<char>)char.TryParse,

                // bit / boolean
                [typeof(bool)] = (Tpd<bool>)bool.TryParse,

                // 8 bit int
                [typeof(byte)] = (Tpd<byte>)byte.TryParse,
                [typeof(sbyte)] = (Tpd<sbyte>)sbyte.TryParse,

                // 16 bit int
                [typeof(short)] = (Tpd<short>)short.TryParse,
                [typeof(ushort)] = (Tpd<ushort>)ushort.TryParse,

                // 32 bit int
                [typeof(int)] = (Tpd<int>)int.TryParse,
                [typeof(uint)] = (Tpd<uint>)uint.TryParse,

                // 64 bit int
                [typeof(long)] = (Tpd<long>)long.TryParse,
                [typeof(ulong)] = (Tpd<ulong>)ulong.TryParse,

                // floating point int
                [typeof(float)] = (Tpd<float>)float.TryParse,
                [typeof(double)] = (Tpd<double>)double.TryParse,
                [typeof(decimal)] = (Tpd<decimal>)decimal.TryParse,

                // time
                [typeof(DateTime)] = (Tpd<DateTime>)DateTime.TryParse,
                [typeof(DateTimeOffset)] = (Tpd<DateTimeOffset>)DateTimeOffset.TryParse,
                [typeof(TimeOnly)] = (Tpd<TimeOnly>)TimeOnly.TryParse,
                [typeof(DateOnly)] = (Tpd<DateOnly>)DateOnly.TryParse,

                // guid
                [typeof(Guid)] = (Tpd<Guid>)Guid.TryParse
            };

            return callback;
        }
    }

    internal static class BaseTypeConverter
    {
        public static TypeConverter[] CreateBaseReaders()
        {
            var callback = new TypeConverter[]
            {
                // char
                new BaseTypeReader<char>(),

                // bit / boolean
                new BaseTypeReader<bool>(),

                // 8 bit int
                new BaseTypeReader<byte>(),
                new BaseTypeReader<sbyte>(),

                // 16 bit int
                new BaseTypeReader<short>(),
                new BaseTypeReader<ushort>(),

                // 32 bit int
                new BaseTypeReader<int>(),
                new BaseTypeReader<uint>(),

                // 64 bit int
                new BaseTypeReader<long>(),
                new BaseTypeReader<ulong>(),

                // floating point int
                new BaseTypeReader<float>(),
                new BaseTypeReader<double>(),
                new BaseTypeReader<decimal>(),

                // time
                new BaseTypeReader<DateTime>(),
                new BaseTypeReader<DateTimeOffset>(),
                new BaseTypeReader<TimeOnly>(),
                new BaseTypeReader<DateOnly>(),

                // guid
                new BaseTypeReader<Guid>(),
            };

            return callback;
        }
    }
}
