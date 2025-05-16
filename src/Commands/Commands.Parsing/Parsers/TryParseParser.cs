namespace Commands.Parsing;

/// <summary>
///     A type parser that can convert a raw string value into a type with a try-parse method. This class cannot be inherited.
/// </summary>
/// <typeparam name="T">The type this parser targets.</typeparam>
/// <param name="parser">The delegate to parse a nullable <see langword="string"/> to <typeparamref name="T"/>.</param>
public sealed class TryParseParser<T>(TryParseParser<T>.ParseDelegate parser) : TypeParser<T>
{
    /// <inheritdoc />
    public override ValueTask<ParseResult> Parse(
        IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        if ((argument is string str && parser(str, out var value)) || parser(argument?.ToString(), out value))
            return Success(value);

        return Error($"The provided value does not match the expected type. Expected {typeof(T).Name}. Got {argument}. At: '{parameter.Name}'");
    }

    /// <summary>
    ///     A delegate that can parse a nullable <see langword="string"/> to a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="str">The raw value to parse.</param>
    /// <param name="value">The out-value of <typeparamref name="T"/>. This value is not <see langword="null"/> when this delegate returns <see langword="true"/></param>
    /// <returns><see langword="true"/> if the parsing was successful, otherwise <see langword="false"/>.</returns>
#if NET8_0_OR_GREATER
    public delegate bool ParseDelegate(string? str, [NotNullWhen(true)] out T? value);
#else
    public delegate bool ParseDelegate(string? str, out T value);
#endif
}

internal static class TryParseParser
{
    public static List<TypeParser> GetBCLTypes()
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
