namespace Commands.Hosting;

/// <summary>
///     A context object used to configure the component provider.
/// </summary>
public sealed class ComponentBuilder
{
    internal readonly List<ResultHandler> ResultHandlers = [];

    /// <summary>
    ///     Configures the globally available options for building components.
    /// </summary>
    /// <remarks>
    ///     This method can be called multiple times, modifying the options for each call.
    /// </remarks>
    /// <param name="configureOptions">An action that configures the creation of new components.</param>
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configureOptions"/> is <see langword="null"/>.</exception>
    public ComponentBuilder Configure(Action<ComponentOptions> configureOptions)
    {
        Assert.NotNull(configureOptions, nameof(configureOptions));

        configureOptions(ComponentOptions.Default);

        return this;
    }

    /// <summary>
    ///     Adds a <see cref="ResultHandler"/> to the provider. This handler will be used to handle the result of commands, the return type of delegates, and the disposal of resources.
    /// </summary>
    /// <remarks>
    ///     <see cref="HandlerDelegate{TContext}"/> is a delegate implementation of <see cref="ResultHandler"/> for which no additional implementation is required. This is the default handler used when no other handlers are provided.
    /// </remarks>
    /// <param name="handler">The instance to add.</param>
    /// <returns>The same <see cref="ComponentBuilder"/> for call-chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is <see langword="null"/>.</exception>
    public ComponentBuilder AddResultHandler(ResultHandler handler)
    {
        Assert.NotNull(handler, nameof(handler));

        ResultHandlers.Add(handler);

        return this;
    }
}
