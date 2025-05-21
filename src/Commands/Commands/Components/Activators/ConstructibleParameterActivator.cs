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

    public int ContextIndex
        => -1;

    public object? Invoke<TContext>(TContext context, Command? command, object?[]? args, ExecutionOptions options)
        where TContext : IContext
        => _ctor.Invoke(args);
}
