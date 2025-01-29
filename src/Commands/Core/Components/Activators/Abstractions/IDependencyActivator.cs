namespace Commands;

/// <summary>
///     An activator for execution dependencies of the command pipeline.
/// </summary>
/// <typeparam name="T">The base type of the implementation this activator is supposed to return.</typeparam>
public interface IDependencyActivator<T>
{
    /// <summary>
    ///     Gets the type of the activator. This is the type that will be returned when the activator is invoked.
    /// </summary>
    /// <remarks>
    ///     When this value is <see langword="null"/>, the activator will return a pre-initialized type instead, provided when this activator was created.
    /// </remarks>
    public Type? Type { get; }

    /// <summary>
    ///     Gets the service definitions that are required to initialize the activator.
    /// </summary>
    /// <remarks>
    ///     This value is empty when no services are required to initialize the returned value. It is <see langword="null"/> when no dependencies are required to create this type.
    /// </remarks>
    public DependencyParameter[]? Dependencies { get; }

    /// <summary>
    ///     Returns the activated instance of the activator.
    /// </summary>
    /// <param name="services">A collection of services from which the target <typeparamref name="T"/> should be returned, or which should be use to resolve the type immediately.</param>
    /// <returns>An instance of <typeparamref name="T"/> which was pre-initialized, provided from the <see cref="IServiceProvider"/> or created using a self-defined injection pattern.</returns>
    public T Activate(IServiceProvider services);
}
