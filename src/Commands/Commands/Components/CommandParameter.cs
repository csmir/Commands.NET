﻿using Commands.Parsing;

namespace Commands;

/// <summary>
///     A parsible command parameter. 
///     This is the base type of all non-deconstructible parameters, and is used to convert raw arguments into their respective types in order to execute the command and its according target.
///     This class cannot be inherited.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class CommandParameter : ICommandParameter
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public Type Type { get; }

    /// <inheritdoc />
    public Type ExposedType { get; }

    /// <inheritdoc />
    public bool IsNullable { get; }

    /// <inheritdoc />
    public bool IsOptional { get; }

    /// <inheritdoc />
    public bool IsRemainder { get; }

    /// <inheritdoc />
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public IParser Parser { get; }

    /// <inheritdoc />
    public int Position { get; }

    /// <inheritdoc />
    public bool IsCollection
        => Type.IsArray;

    internal CommandParameter(
        ParameterInfo parameterInfo, ComponentOptions options)
    {
        ExposedType = parameterInfo.ParameterType;

        var underlying = Nullable.GetUnderlyingType(parameterInfo.ParameterType);
        var attributes = parameterInfo.GetAttributes(false);

        if (underlying != null)
        {
            IsNullable = true;
            Type = underlying;
        }
        else
        {
            IsNullable = false;
            Type = parameterInfo.ParameterType;
        }

        Position = parameterInfo.Position;

        if (parameterInfo.IsOptional)
            IsOptional = true;
        else
            IsOptional = false;

        if (attributes.Contains<RemainderAttribute>() || attributes.Contains<ParamArrayAttribute>())
            IsRemainder = true;
        else
            IsRemainder = false;

        // Assign the parser defined on the parameter if it exists, otherwise use the one defined in the configuration.
        Parser = attributes.FirstOrDefault<IParser>() ?? options.GetParser(Type);

        Attributes = [.. attributes];

        Name = attributes.FirstOrDefault<NameAttribute>()?.Name ?? parameterInfo.Name ?? "";
    }

    /// <inheritdoc />
    public string GetFullName()
    {
        if (IsOptional)
            return $"[{Name}]";

        if (IsRemainder)
            return $"({Name}...)";

        return $"<{Name}>";
    }

    /// <inheritdoc />
    public float GetScore()
    {
        var score = 1.0f;

        if (IsOptional)
            score -= 0.5f;

        if (IsRemainder)
            score -= 0.25f;

        if (IsNullable)
            score -= 0.25f;

        return score;
    }

    /// <inheritdoc />
    public ValueTask<ParseResult> Parse(IContext context, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        // Fast path for matching instances of certain types.
        if (Type.IsInstanceOfType(value))
            return ParseResult.FromSuccess(value);

        if (value is null or "null")
        {
            if (IsNullable)
                return ParseResult.FromSuccess(null);

            return ParseResult.FromError(new ParserException(Parser, "A null (or \"null\") value was attempted to be provided to a non-nullable command parameter."));
        }

        return Parser?.Parse(context, this, value, services, cancellationToken) ?? ParseResult.FromSuccess(value.ToString());
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
        => obj is ICommandSegment scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public override string ToString()
        => ToString(false);

    /// <inheritdoc cref="ToString()"/>
    /// <param name="includeArgumentNames">Defines whether the argument signatures should be named or not.</param>
    public string ToString(bool includeArgumentNames)
        => $"{Type.Name}{(includeArgumentNames ? Name : "")}";
}
