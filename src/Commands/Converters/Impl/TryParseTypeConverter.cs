using Commands.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Converters
{
    /// <summary>
    ///     A type converter that can convert a raw string value into a type with a try-parse method.
    /// </summary>
    /// <typeparam name="T">The type this converter targets.</typeparam>
    public sealed class TryParseTypeConverter<T> : TypeConverter<T>
    {
        private readonly ParseDelegate _parser;

        /// <summary>
        ///     Creates a new instance of the <see cref="TryParseTypeConverter{T}" /> class, with the specified parsing delegate. This delegate is a try-parse method of the target type.
        /// </summary>
        /// <param name="parser">The delegate to parse a nullable <see langword="string"/> to <typeparamref name="T"/>.</param>
        public TryParseTypeConverter(ParseDelegate parser)
        {
            _parser = parser;
        }

        /// <inheritdoc />
        public override async ValueTask<ConvertResult> Evaluate(
            CallerContext consumer, IArgument parameter, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (_parser(value?.ToString(), out var result))
                return Success(result);

            return Error($"The provided value does not match the expected type. Expected {typeof(T).Name}, got {value}. At: '{parameter.Name}'");
        }

        /// <summary>
        ///     A delegate that can parse a nullable <see langword="string"/> to a value of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="str">The raw value to parse.</param>
        /// <param name="value">The out-value of <typeparamref name="T"/>. This value is not <see langword="null"/> when this delegate returns <see langword="true"/></param>
        /// <returns><see langword="true"/> if the parsing was successful, otherwise <see langword="false"/>.</returns>
        public delegate bool ParseDelegate(string? str, [NotNullWhen(true)] out T value);
    }

    internal static class TryParseTypeConverter
    {
        public static List<TypeConverter> CreateBaseConverters()
            => [
                // char
                new TryParseTypeConverter<char>(char.TryParse),

                // bit / boolean
                new TryParseTypeConverter<bool>(bool.TryParse),

                // 8 bit int
                new TryParseTypeConverter<byte>(byte.TryParse),
                new TryParseTypeConverter<sbyte>(sbyte.TryParse),

                // 16 bit int
                new TryParseTypeConverter<short>(short.TryParse),
                new TryParseTypeConverter<ushort>(ushort.TryParse),

                // 32 bit int
                new TryParseTypeConverter<int>(int.TryParse),
                new TryParseTypeConverter<uint>(uint.TryParse),

                // 64 bit int
                new TryParseTypeConverter<long>(long.TryParse),
                new TryParseTypeConverter<ulong>(ulong.TryParse),

                // floating point int
                new TryParseTypeConverter<float>(float.TryParse),
                new TryParseTypeConverter<double>(double.TryParse),
                new TryParseTypeConverter<decimal>(decimal.TryParse),

                // time
                new TryParseTypeConverter<DateTime>(DateTime.TryParse),
                new TryParseTypeConverter<DateTimeOffset>(DateTimeOffset.TryParse),

                // guid
                new TryParseTypeConverter<Guid>(Guid.TryParse),
            ];
    }
}
