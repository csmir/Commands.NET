namespace Commands;

/// <summary>
///     Reveals information about a service parameter.
/// </summary>
[DebuggerDisplay("{ToString()}")]
public sealed class DependencyParameter : IParameter, IAttributeContainer
{
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

        ExposedType = parameterInfo.ParameterType;

        Attributes = [.. parameterInfo.GetAttributes(false)];
    }

    /// <inheritdoc />
    public override string ToString()
        => Type.Name;
}
