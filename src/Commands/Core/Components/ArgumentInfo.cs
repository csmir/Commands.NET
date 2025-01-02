using Commands.Conversion;
using System.Diagnostics;
using System.Reflection;

namespace Commands;

/// <inheritdoc />
[DebuggerDisplay("{ToString()}")]
public sealed class ArgumentInfo : IArgument
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
    public TypeParser Parser { get; }

    /// <inheritdoc />
    public int Position { get; }

    /// <inheritdoc />
    public bool IsCollection
        => Type.IsArray;

    internal ArgumentInfo(
        ParameterInfo parameterInfo, string? name, ComponentConfiguration configuration)
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

        if (attributes.Contains<RemainderAttribute>(false) || attributes.Contains<ParamArrayAttribute>(false))
            IsRemainder = true;
        else
            IsRemainder = false;

        Parser = configuration.GetParser(Type);
        Attributes = attributes.ToArray();

        if (!string.IsNullOrEmpty(name))
            Name = name!;
        else
            Name = parameterInfo.Name ?? "";
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
    public ValueTask<ParseResult> Parse(ICallerContext caller, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        // Fast path for matching instances of certain types.
        if (Type.IsInstanceOfType(value))
            return ParseResult.FromSuccess(value);

        if (value is null or "null")
        {
            if (IsNullable)
                return ParseResult.FromSuccess(null);

            return ParseResult.FromError(new ParseException("A null (or \"null\") value was attempted to be provided to a non-nullable command parameter."));
        }

        return Parser?.Parse(caller, this, value, services, cancellationToken) ?? ParseResult.FromSuccess(value.ToString());
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
        => obj is IScorable scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public override string ToString()
        => ToString(false);

    /// <inheritdoc cref="ToString()"/>
    /// <param name="includeArgumentNames">Defines whether the argument signatures should be named or not.</param>
    public string ToString(bool includeArgumentNames)
        => $"{Type.Name}{(includeArgumentNames ? Name : "")}";
}
