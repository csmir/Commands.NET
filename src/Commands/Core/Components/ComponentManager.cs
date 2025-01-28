namespace Commands;

/// <summary>
///     A concurrent implementation of the mechanism that allows commands to be executed using a provided set of arguments. This class cannot be inherited.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public sealed class ComponentManager : ComponentCollection, IExecutionProvider
{
    private readonly ResultHandler[] _handlers;

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentManager"/> with the specified handlers.
    /// </summary>
    /// <remarks>
    ///     This overload supports enumerable service injection in order to create a manager from service definitions.
    /// </remarks>
    /// <param name="handlers">A collection of handlers for post-execution processing of retrieved command input.</param>
    public ComponentManager(IEnumerable<ResultHandler> handlers)
        : this(handlers.ToArray()) { }

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentManager"/> with the specified handlers.
    /// </summary>
    /// <param name="handlers">A collection of handlers for post-execution processing of retrieved command input.</param>
    public ComponentManager(params ResultHandler[] handlers)
        : base()
    {
        _handlers = handlers;

        // A default handler is added if none are provided, which allows the command result to be processed with no further implications.
        if (handlers.Length < 0)
            _handlers = [new DelegateResultHandler<ICallerContext>()];
    }

    /// <inheritdoc />
    public override IEnumerable<IComponent> Find(ArgumentDictionary args)
    {
        List<IComponent> discovered = [];

        var enumerator = GetSpanEnumerator();

        while (enumerator.MoveNext())
        {
            if (!args.TryGetElementAt(0, out var value) || !enumerator.Current.Names.Contains(value))
                continue;

            if (enumerator.Current is CommandGroup group)
                discovered.AddRange(group.Find(args));

            else
                discovered.Add(enumerator.Current);
        }

        return discovered;
    }

    /// <inheritdoc />
    public Task Execute<TContext>(TContext context, CommandOptions? options = null)
        where TContext : ICallerContext
    {
        options ??= new CommandOptions();

        var task = ExecutePipelineTask(context, options);

        task.ContinueWith(async task =>
        {
            var result = await task.ConfigureAwait(false);

            foreach (var handler in _handlers)
                await handler.HandleResult(context, result, options.Services, options.CancellationToken).ConfigureAwait(false);

        }, options.CancellationToken);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IExecuteResult> ExecuteBlocking<TContext>(TContext context, CommandOptions? options = null)
        where TContext : ICallerContext
    {
        options ??= new CommandOptions();

        var result = await ExecutePipelineTask(context, options).ConfigureAwait(false);

        foreach (var handler in _handlers)
            await handler.HandleResult(context, result, options.Services, options.CancellationToken).ConfigureAwait(false);

        return result;
    }

    private async Task<IExecuteResult> ExecutePipelineTask<TContext>(TContext caller, CommandOptions options)
        where TContext : ICallerContext
    {
        options.Manager = this;

        IExecuteResult? result = null;

        var components = Find(caller.Arguments);

        foreach (var component in components)
        {
            if (component is Command command)
            {
                result = await command.Run(caller, options).ConfigureAwait(false);

                if (!result.Success)
                    continue;

                break;
            }

            result ??= new SearchResult(new CommandRouteIncompleteException(component));
        }

        return result ?? new SearchResult(new CommandNotFoundException());
    }

    #region Initializers

    /// <inheritdoc cref="From(IComponentProperties[])"/>
    public static ComponentManagerProperties With
        => new();

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="ComponentManager"/>.
    /// </summary>
    /// <param name="components">The components to add.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static ComponentManagerProperties From(params IComponentProperties[] components)
        => new ComponentManagerProperties().Components(components);

    #endregion
}
