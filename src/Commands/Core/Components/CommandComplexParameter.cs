﻿using Commands.Conversion;
using System.Reflection;

namespace Commands;

/// <summary>
///     Reveals information about a type with a defined complex constructor.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public class CommandComplexParameter : ICommandParameter, ICommandSegment, IParameterCollection
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
    public TypeParser? Parser { get; } = null;

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
    internal CommandComplexParameter(
        ParameterInfo parameterInfo, string? name, ComponentConfiguration configuration)
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

        Activator = new ComplexParameterActivator(Type);

        var parameters = Activator.Target.BuildArguments(false, configuration);

        if (parameters.Length == 0)
            throw new NotSupportedException($"Complex argument of type {Type} must have at least one parameter.");

        (MinLength, MaxLength) = parameters.GetLength();

        Parameters = parameters;

        Attributes = attributes.ToArray();

        ExposedType = parameterInfo.ParameterType;

        if (!string.IsNullOrEmpty(name))
            Name = name!;
        else
            Name = parameterInfo.Name ?? "";
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
    public bool HasAttribute<T>()
        where T : Attribute
        => Attributes.Contains<T>(true);

    /// <inheritdoc />
    public T? GetAttribute<T>(T? defaultValue = default)
        where T : Attribute
        => Attributes.FirstOrDefault<T>() ?? defaultValue;

    /// <inheritdoc />
    public ValueTask<ParseResult> Parse(ICallerContext caller, object? value, IServiceProvider services, CancellationToken cancellationToken)
        => throw new NotSupportedException("Complex arguments do not support parsing.");

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
}
