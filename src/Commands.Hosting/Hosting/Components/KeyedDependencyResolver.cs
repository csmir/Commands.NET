﻿namespace Commands.Hosting;

internal sealed class KeyedDependencyResolver(IServiceProvider provider) : IDependencyResolver
{
    public object? GetService(DependencyParameter dependency)
    {
        if (provider is IKeyedServiceProvider keyedProvider && dependency.Attributes.FirstOrDefault<FromKeyedServicesAttribute>() is { Key: var key })
            return keyedProvider.GetKeyedService(dependency.Type, key);

        return provider.GetService(dependency.Type);
    }
}
