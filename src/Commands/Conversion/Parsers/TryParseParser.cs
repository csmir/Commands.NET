﻿using System.Diagnostics.CodeAnalysis;

namespace Commands.Conversion
{
    /// <summary>
    ///     A type converter that can convert a raw string value into a type with a try-parse method. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="T">The type this converter targets.</typeparam>
    public sealed class TryParseParser<T> : TypeParser<T>
    {
        private readonly ParseDelegate _parser;

        /// <summary>
        ///     Creates a new instance of the <see cref="TryParseParser{T}" /> class, with the specified parsing delegate. This delegate is a try-parse method of the target type.
        /// </summary>
        /// <param name="parser">The delegate to parse a nullable <see langword="string"/> to <typeparamref name="T"/>.</param>
        public TryParseParser(ParseDelegate parser)
        {
            _parser = parser;
        }

        /// <inheritdoc />
        public override async ValueTask<ConvertResult> Parse(
            ICallerContext caller, IArgument parameter, object? value, IServiceProvider services, CancellationToken cancellationToken)
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

    internal static class TryParseParser
    {
        public static List<TypeParser> CreateBaseConverters()
            => [
                // char
                new TryParseParser<char>(char.TryParse),

                // bit / boolean
                new TryParseParser<bool>(bool.TryParse),

                // 8 bit int
                new TryParseParser<byte>(byte.TryParse),
                new TryParseParser<sbyte>(sbyte.TryParse),

                // 16 bit int
                new TryParseParser<short>(short.TryParse),
                new TryParseParser<ushort>(ushort.TryParse),

                // 32 bit int
                new TryParseParser<int>(int.TryParse),
                new TryParseParser<uint>(uint.TryParse),

                // 64 bit int
                new TryParseParser<long>(long.TryParse),
                new TryParseParser<ulong>(ulong.TryParse),

                // floating point int
                new TryParseParser<float>(float.TryParse),
                new TryParseParser<double>(double.TryParse),
                new TryParseParser<decimal>(decimal.TryParse),

                // time
                new TryParseParser<DateTime>(DateTime.TryParse),
                new TryParseParser<DateTimeOffset>(DateTimeOffset.TryParse),

                // guid
                new TryParseParser<Guid>(Guid.TryParse),
            ];
    }
}