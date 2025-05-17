using Commands.Parsing;

namespace Commands;

/// <summary>
///     A deconstructed parameter. This is the type that represents a type of which a constructor is considered part of a command signature, and is reconstructed using the discovered constructor for that type.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class ConstructibleParameter : ICommandParameter, IParameterCollection
{
    /// <inheritdoc />
    public IActivator Activator { get; }

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
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public ICommandParameter[] Parameters { get; }

    /// <inheritdoc />
    public int MinLength { get; }

    /// <inheritdoc />
    public int MaxLength { get; }

    /// <inheritdoc />
    public IParser? Parser { get; } = null;

    /// <inheritdoc />
    public int Position { get; }

    /// <inheritdoc />
    public bool IsCollection
        => false;

    /// <inheritdoc />
    public bool IsRemainder
        => false;

    /// <inheritdoc />
    public bool HasParameters
        => Parameters.Length > 0;

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2072", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
    internal ConstructibleParameter(
        ParameterInfo parameterInfo, ComponentOptions configuration)
    {
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

        Activator = new ConstructibleParameterActivator(Type);

        var parameters = ComponentUtilities.GetParameters(Activator, configuration);

        if (parameters.Length == 0)
            throw new ParameterFormatException($"Deconstruct-marked parameter of type {Type} must have at least one parameter in one of its public constructors.");

        (MinLength, MaxLength) = parameters.GetLength();

        Parameters = parameters;

        Attributes = [.. attributes];

        ExposedType = parameterInfo.ParameterType;

        Name = attributes.FirstOrDefault<NameAttribute>()?.Name ?? parameterInfo.Name ?? "";
    }

    /// <inheritdoc />
    public string GetFullName()
        => string.Join(" ", Parameters.Select(x => x.GetFullName()));

    /// <inheritdoc />
    public float GetScore()
    {
        var score = 1.0f;

        if (IsOptional)
            score -= 0.5f;

        if (IsNullable)
            score -= 0.25f;

        foreach (var arg in Parameters)
            score += arg.GetScore();

        return score;
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
        => $"{Type.Name}{(includeArgumentNames ? $" {Name} " : "")}({string.Join<ICommandParameter>(", ", Parameters)})";

    ValueTask<ParseResult> ICommandParameter.Parse(IContext context, object? value, IServiceProvider services, CancellationToken cancellationToken)
        => throw new NotSupportedException("Deconstruct-marked parameters do not support parsing.");
}
