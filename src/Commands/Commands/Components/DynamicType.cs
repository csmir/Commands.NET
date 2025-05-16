namespace Commands;

/// <summary>
///     A readonly representation of a <see cref="Type"/>, enforcing dynamic access to its public methods, constructors and nested types.
/// </summary>
/// <remarks>
///     This struct is implicitly convertible from and to <see cref="Type"/> and can be used in place of a <see cref="Type"/> in most cases.
/// </remarks>
[DebuggerDisplay("{Value}")]
public readonly struct DynamicType
{
    /// <summary>
    ///     Gets the <see cref="Type"/> that this <see cref="DynamicType"/> represents.
    /// </summary>
#if NET8_0_OR_GREATER
    [field: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public readonly Type Value;

    /// <summary>
    ///     Creates a new <see cref="DynamicType"/> from the provided <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The type that this <see cref="DynamicType"/> should contain.</param>
    public DynamicType(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
        => Value = type;

    /// <summary>
    ///     Implicitly converts a <see cref="DynamicType"/> to a <see cref="Type"/>.
    /// </summary>
    /// <param name="definition"></param>
#if NET8_0_OR_GREATER
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public static implicit operator Type(DynamicType definition)
        => definition.Value;

    /// <summary>
    ///     Implicitly converts a <see cref="Type"/> to a <see cref="DynamicType"/>.
    /// </summary>
    /// <param name="type"></param>
    public static implicit operator DynamicType(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type)
        => new(type);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is DynamicType other && Value == other.Value;

    /// <inheritdoc />
    public override int GetHashCode()
        => Value.GetHashCode();

    /// <inheritdoc />
    public static bool operator ==(DynamicType left, DynamicType right)
        => left.Equals(right);

    /// <inheritdoc />
    public static bool operator !=(DynamicType left, DynamicType right)
        => !left.Equals(right);
}
