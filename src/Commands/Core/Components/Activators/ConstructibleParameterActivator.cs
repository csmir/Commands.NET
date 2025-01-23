namespace Commands;

internal readonly struct ConstructibleParameterActivator(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
        Type type) : IActivator
{
    private readonly ConstructorInfo _ctor = type.GetAvailableConstructor();

    public MethodBase Target
        => _ctor;

    public bool HasContext
        => false;

    public object? Invoke<T>(T caller, Command? command, object?[]? args, CommandOptions options) where T : ICallerContext
        => _ctor.Invoke(args);
}
