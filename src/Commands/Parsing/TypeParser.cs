﻿namespace Commands.Parsing;

/// <summary>
///     A delegate-based type parser that can be used to parse a type from a raw argument.
/// </summary>
/// <typeparam name="TConvertible">The target type of the parser.</typeparam>
/// <param name="parseDelegate">The execution delegate which will be triggered when a value is to be converted to the provided argument.</param>
public sealed class DelegateTypeParser<TConvertible>(
    Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> parseDelegate)
    : TypeParser<TConvertible>
{
    /// <inheritdoc />
    public override ValueTask<ParseResult> Parse(ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        => parseDelegate(caller, argument, value, services);
}

/// <inheritdoc />
/// <typeparam name="TConvertible">The type this <see cref="TypeParser{T}"/> should parse into.</typeparam>
public abstract class TypeParser<TConvertible> : TypeParser
{
    /// <inheritdoc />
    public override Type Type { get; } = typeof(TConvertible);

    /// <summary>
    ///     Creates a new <see cref="ParseResult"/> representing a successful parse operation.
    /// </summary>
    /// <param name="value">The value parsed from a raw argument into the target type of this parser.</param>
    /// <returns>A <see cref="ParseResult"/> representing the successful parse operation.</returns>
    public virtual ParseResult Success(TConvertible? value)
        => base.Success(value);
}

/// <summary>
///     An abstract type that can be implemented to create custom type parsing from a command query argument.
/// </summary>
public abstract class TypeParser : ITypeParser
{
    /// <inheritdoc />
    public abstract Type Type { get; }

    /// <inheritdoc />
    public abstract ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken);

    /// <inheritdoc />
    public ParseResult Error(string error)
    {
        Assert.NotNullOrEmpty(error, nameof(error));

        return ParseResult.FromError(new ParserException(this, error));
    }

    /// <inheritdoc />
    public ParseResult Success(object? value)
        => ParseResult.FromSuccess(value);

    #region Initializers

    /// <inheritdoc cref="From{T}(TryParseParser{T}.ParseDelegate)"/>
    public static TypeParserBuilder<T> For<T>()
        => new();

    /// <inheritdoc cref="From{T}(TryParseParser{T}.ParseDelegate)"/>
    public static TypeParserBuilder<T> From<T>(Func<ICallerContext, ICommandParameter, object?, IServiceProvider, ValueTask<ParseResult>> executionDelegate)
        => new TypeParserBuilder<T>().AddDelegate(executionDelegate);

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="TypeParser"/>.
    /// </summary>
    /// <param name="executionDelegate">The delegate that should be executed when the parser is invoked.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static TypeParserBuilder<T> From<T>(TryParseParser<T>.ParseDelegate executionDelegate)
        => new TypeParserBuilder<T>().AddDelegate(executionDelegate);
    internal static IEnumerable<TypeParser> CreateDefaults()
    {
        var list = TryParseParser.CreateBaseParsers();

        list.Add(new TimeSpanParser());
        list.Add(new ColorParser());
        list.Add(new ObjectParser());
        list.Add(new StringParser());

        return list;
    }

    #endregion
}
