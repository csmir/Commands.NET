namespace Commands;

/// <summary>
///     Represents an activator that can create an instance of a constructible type, being a parameter marked with <see cref="DeconstructAttribute"/>. This class cannot be inherited.
/// </summary>
public sealed class ConstructibleParameterActivator : IActivator
{
    private readonly ConstructorInfo _ctor;

    /// <inheritdoc />
    public MethodBase Target
        => _ctor;

    /// <inheritdoc />
    public bool HasContext
        => false;

    internal ConstructibleParameterActivator(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type type)
    {
        var ctors = type.GetAvailableConstructors();

        _ctor = ctors.First();
    }

    /// <inheritdoc />
    public object? Invoke<T>(T caller, Command? command, object?[]? args, CommandOptions options) where T : ICallerContext
        => _ctor.Invoke(args);
}
