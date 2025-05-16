namespace Commands;

/// <summary>
///     A resolver that can be used to request dependencies from a <see cref="IServiceProvider"/> used in creating new instances of <see cref="CommandModule"/>.
/// </summary>
public interface IDependencyResolver
{
    /// <summary>
    ///     Requests a service from the <see cref="IServiceProvider"/> using the provided <see cref="DependencyParameter"/>'s defined variables.
    /// </summary>
    /// <param name="dependency">The parameter holding the necessary information to request a value from the dependency resolver.</param>
    /// <returns>An instance of the service requested from the <see cref="IServiceProvider"/> used by the provided <see cref="DependencyParameter"/>.</returns>
    public object? GetService(DependencyParameter dependency);
}
