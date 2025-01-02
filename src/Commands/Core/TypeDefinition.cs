namespace Commands;

/// <summary>
///     A readonly representation of a <see cref="Type"/>, enforcing dynamic access for all its members.
/// </summary>
/// <remarks>
///     This struct is implicitly convertible to <see cref="Type"/> and can be used in place of a <see cref="Type"/> in most cases.
/// </remarks>
[DebuggerDisplay("{Value}")]
public readonly struct TypeDefinition
{
    /// <summary>
    ///     Gets the <see cref="Type"/> that this <see cref="TypeDefinition"/> represents.
    /// </summary>
#if NET8_0_OR_GREATER
    [field: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public readonly Type Value;

    /// <summary>
    ///     Creates a new <see cref="TypeDefinition"/> from the provided <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type that this <see cref="TypeDefinition"/> should contain.</param>
    public TypeDefinition(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
        => Value = type;

    /// <summary>
    ///     Implicitly converts a <see cref="TypeDefinition"/> to a <see cref="Type"/>.
    /// </summary>
    /// <param name="definition"></param>
#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public static implicit operator Type(TypeDefinition definition)
        => definition.Value;

    /// <summary>
    ///     Implicitly converts a <see cref="Type"/> to a <see cref="TypeDefinition"/>.
    /// </summary>
    /// <param name="type"></param>
    public static implicit operator TypeDefinition(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
        => new(type);
}
