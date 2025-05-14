namespace Commands;

internal readonly struct DefaultDependencyResolver(IServiceProvider serviceProvider) : IDependencyResolver
{
    public object? GetService(DependencyParameter dependency)
        => serviceProvider.GetService(dependency.Type);
}
