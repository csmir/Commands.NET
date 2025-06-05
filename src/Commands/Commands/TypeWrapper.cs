using System.ComponentModel;

namespace Commands;

/// <summary>
///     An immutable wrapper for a <see cref="Type"/> that can be used to ensure that the type is Native-AOT compatible. Do not use this type directly.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct TypeWrapper
{
    /// <summary></summary>
#if NET8_0_OR_GREATER
    [field: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public readonly Type Value;

    /// <summary></summary>
    public TypeWrapper(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
        => Value = type;

    /// <summary></summary>
#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public static implicit operator Type(TypeWrapper definition)
        => definition.Value;

    /// <summary></summary>
    public static implicit operator TypeWrapper(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
        => new(type);
}
