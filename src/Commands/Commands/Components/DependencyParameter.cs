namespace Commands;

/// <summary>
///     Reveals information about a service parameter. This class cannot be inherited.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class DependencyParameter : IParameter
{
    /// <inheritdoc />
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
#endif
    public Type Type { get; }

    /// <inheritdoc />
    public Type ExposedType { get; }

    /// <inheritdoc />
    public bool IsNullable { get; }

    /// <inheritdoc />
    public bool IsOptional { get; }

    /// <inheritdoc />
    public int Position { get; }

    /// <summary>
    ///     Gets all attributes on the current object.
    /// </summary>
    public Attribute[] Attributes { get; }

    internal DependencyParameter(
        ParameterInfo parameterInfo)
    {
        var underlying = Nullable.GetUnderlyingType(parameterInfo.ParameterType);

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

        if (parameterInfo.IsOptional)
            IsOptional = true;
        else
            IsOptional = false;

        Position = parameterInfo.Position;
        ExposedType = parameterInfo.ParameterType;

        Attributes = [.. parameterInfo.GetAttributes(false)];
    }

    /// <inheritdoc />
    public override string ToString()
        => Type.Name;
}
