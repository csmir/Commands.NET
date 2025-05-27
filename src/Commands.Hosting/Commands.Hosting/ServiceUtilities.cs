using System.Runtime.CompilerServices;

namespace Commands.Hosting;

/// <summary>
///     A static class containing methods for configuring a <see cref="IServiceProvider"/> with Commands.NET functionality.
/// </summary>
public static class ServiceUtilities
{
    /// <summary>
    ///     Adds a <see cref="IComponentProvider"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>
    ///     This method will remove any existing <see cref="IComponentProvider"/> and <see cref="ICommandExecutionFactory"/> from the collection before adding newly configured instances. Additionally, it configures a <see cref="IExecutionScope"/> and <see cref="IContextAccessor{TContext}"/> under scoped context.
    /// </remarks>
    /// <param name="services">The <see cref="IServiceProvider"/> to add the configured services to.</param>
    /// <param name="configureAction"></param>
    /// <returns>The same <see cref="IServiceCollection"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configureAction"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddComponentProvider(this IServiceCollection services, Action<ComponentBuilder> configureAction)
    {
        Assert.NotNull(services, nameof(services));
        Assert.NotNull(configureAction, nameof(configureAction));

        var properties = new ComponentBuilder();

        configureAction(properties);

        AddComponentProvider(services, properties);

        return services;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddComponentProvider(IServiceCollection services, ComponentBuilder properties)
        => properties.DefineServices(services);
}
