namespace Commands;

/// <summary>
///     A provider hosting a <see cref="ComponentTree"/> that can be executed through a pipeline. This class can be implemented to provide custom behavior.
/// </summary>
/// <remarks>
///     To learn more about how to use this type, visit the Commands.NET documentation: <see href="https://github.com/csmir/Commands.NET/wiki"/>.
/// </remarks>
public class ComponentProvider : IComponentProvider
{
    private readonly ResultHandler[] _handlers;

    /// <inheritdoc />
    public ComponentTree Components { get; }

    /// <summary>
    ///     Creates a new instance of the <see cref="ComponentProvider"/> which implements result handling logic according to the provided <see cref="ResultHandler"/> instances.
    /// </summary>
    /// <remarks>
    ///     If no handlers are provided, a default handler is added which allows the command result to be processed with no further implications.
    /// </remarks>
    /// <param name="handlers">An extensible array of <see cref="ResultHandler"/> implementations, which provide custom behavior for handling the result of commands, the return type of delegates, and the disposal of resources.</param>
    public ComponentProvider(params ResultHandler[] handlers)
    {
        Assert.NotNull(handlers, nameof(handlers));

        Components = [];

        // A default handler is added if none are provided, which allows the command result to be processed with no further implications.
        if (handlers.Length == 0)
            _handlers = [ResultHandler.Default];
        else
            _handlers = handlers;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ComponentProvider"/> which implements result handling logic according to the provided <see cref="ResultHandler"/> instances and uses the provided <see cref="ComponentTree"/> as the source of components.
    /// </summary>
    /// <param name="components">A pre-initialized set of components which this provider should treat as the instance of <see cref="Components"/> targetted to find and execute commands from.</param>
    /// <param name="handlers">An extensible array of <see cref="ResultHandler"/> implementations, which provide custom behavior for handling the result of commands, the return type of delegates, and the disposal of resources.</param>
    public ComponentProvider(ComponentTree components, params ResultHandler[] handlers)
        : this(handlers)
    {
        Assert.NotNull(components, nameof(components));

        Components = components;
    }

    /// <inheritdoc />
    public virtual Task Execute<TContext>(TContext context, ExecutionOptions? options = null)
        where TContext : class, ICallerContext
    {
        options ??= ExecutionOptions.Default;

        if (options.ExecuteAsynchronously)
        {
            _ = Work(context, options);

            return Task.CompletedTask;
        }

        return Work(context, options);
    }

    /// <summary>
    ///     A protected method that fires the command pipeline using the provided context and options, and handles the returned results according to the defined <see cref="ResultHandler"/> implementations added at creation.
    /// </summary>
    /// <remarks>
    ///      This method is called by the <see cref="Execute{TContext}"/> method after determining the execution approach, and can be overridden to provide custom behavior.
    /// </remarks>
    /// <typeparam name="TContext">The implementation type of <see cref="ICallerContext"/> which represents the caller (or source) of the execution request.</typeparam>
    /// <param name="context">The implementation of <see cref="ICallerContext"/> which represents the caller (or source) of the execution request.</param>
    /// <param name="options">The options used to customize the command execution pipeline in accordance to the context and requirements of execution.</param>
    /// <returns>An awaitable <see cref="Task"/> representing the operation.</returns>
    protected virtual async Task Work<TContext>(TContext context, ExecutionOptions options)
        where TContext : class, ICallerContext
    {
        options.Provider ??= this;

        IResult? result = null;

        var components = Components.Find(context.Arguments);

        foreach (var component in components)
        {
            if (component is Command command)
            {
                result = await command.Run(context, options).ConfigureAwait(false);

                if (result.Success)
                    break;
            }

            result ??= new SearchResult(new CommandRouteIncompleteException(component));
        }

        result ??= new SearchResult(new CommandNotFoundException());
        
        foreach (var handler in _handlers)
            await handler.HandleResult(context, result, options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);
    }
}
