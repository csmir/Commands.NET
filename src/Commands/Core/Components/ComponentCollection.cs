﻿namespace Commands;

/// <summary>
///     A concurrent implementation of the mechanism that allows commands to be executed using a provided set of arguments. This class cannot be inherited.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public sealed class ComponentCollection : ComponentCollectionBase, IExecutionProvider
{
    private readonly ResultHandler[] _handlers;

    /// <summary>
    ///     Gets the configuration for this component manager.
    /// </summary>
    public ComponentConfiguration Configuration { get; }

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentCollection"/> with the specified handlers.
    /// </summary>
    /// <remarks>
    ///     This overload supports enumerable service injection in order to create a manager from service definitions.
    /// </remarks>
    /// <param name="configuration">The configuration for this component manager.</param>
    /// <param name="handlers">A collection of handlers for post-execution processing of retrieved command input.</param>
    public ComponentCollection(ComponentConfiguration configuration, IEnumerable<ResultHandler> handlers)
        : this(handlers.ToArray())
    {
        Configuration = configuration;
    }

    /// <summary>
    ///     Creates a new instance of <see cref="ComponentCollection"/> with the specified handlers.
    /// </summary>
    /// <param name="handlers">A collection of handlers for post-execution processing of retrieved command input.</param>
    public ComponentCollection(params ResultHandler[] handlers)
    {
        _handlers = handlers;

        Configuration = ComponentConfiguration.Empty;

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
    public Task<IResult?> Execute<TContext>(TContext context, CommandOptions? options = null)
        where TContext : class, ICallerContext
    {
        async Task<IResult> PostExecuteAwait(Task<IResult> result, CommandOptions options)
        {
            var output = await result.ConfigureAwait(false);

            foreach (var handler in _handlers)
                await handler.HandleResult(context, output, options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);

            return output;
        }

        options ??= CommandOptions.Default;

        var task = ExecutePipelineTask(context, options);

        if (!options.ExecuteAsynchronously)
            return PostExecuteAwait(task, options)!;

        task.ContinueWith(task => PostExecuteAwait(task, options), options.CancellationToken);

        return Task.FromResult<IResult?>(null);
    }

    /// <summary>
    ///     Adds a collection of types to the component manager.
    /// </summary>
    /// <remarks>
    ///     This operation will add implementations of <see cref="CommandModule"/> and <see cref="CommandModule{T}"/>, that are public and non-abstract to the current manager.
    /// </remarks>
    /// <param name="types">A collection of types to filter and add to the manager, where possible.</param>
    /// <returns>The number of added components; or 0 if no components are added.</returns>
    public int AddRange(IEnumerable<Type> types)
    {
        var components = ComponentUtilities.GetComponents(types, Configuration);

        return AddRange(components);
    }

    private async Task<IResult> ExecutePipelineTask<TContext>(TContext caller, CommandOptions options)
        where TContext : class, ICallerContext
    {
        options.Manager = this;

        IResult? result = null;

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
    public static ComponentCollectionProperties With
        => new();

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="ComponentCollection"/>.
    /// </summary>
    /// <param name="components">The components to add.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static ComponentCollectionProperties From(params IComponentProperties[] components)
        => new ComponentCollectionProperties().Components(components);

    #endregion
}
