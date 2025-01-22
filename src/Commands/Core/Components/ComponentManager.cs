namespace Commands;

/// <summary>
///     A concurrent implementation of the mechanism that allows commands to be executed using a provided set of arguments. This class cannot be inherited.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
public sealed class ComponentManager : ComponentCollection, IExecutionProvider
{
    private readonly bool _handlersAvailable;
    private readonly ResultHandler[] _handlers;

    /// <summary>
    ///     Initializes a new instance of <see cref="ComponentManager"/> with the specified handlers.
    /// </summary>
    /// <param name="handlers">A collection of handlers for post-execution processing of retrieved command input.</param>
    public ComponentManager(IEnumerable<ResultHandler> handlers)
        : base()
    {
        var arr = handlers.ToArray();

        _handlersAvailable = arr.Length > 0;
        _handlers = arr;
    }

    /// <inheritdoc />
    public override IEnumerable<IComponent> Find(ArgumentArray args)
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
    public void TryExecute<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecute(caller, ArgumentArray.From(args), options);

    /// <inheritdoc />
    public void TryExecute<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, ArgumentArray.From(args), options).Wait();

    /// <inheritdoc />
    public void TryExecute<T>(T caller, ArgumentArray args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, args, options).Wait();

    /// <inheritdoc />
    public Task TryExecuteAsync<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, ArgumentArray.From(args), options);

    /// <inheritdoc />
    public Task TryExecuteAsync<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, ArgumentArray.From(args), options);

    /// <inheritdoc />
    public Task TryExecuteAsync<T>(T caller, ArgumentArray args, CommandOptions? options = null)
        where T : ICallerContext
    {
        options ??= new CommandOptions();

        var task = ExecutePipelineTask(caller, args, options);

        if (options.AsynchronousExecution)
            return Task.CompletedTask;

        return task;
    }

    private async Task ExecutePipelineTask<T>(T caller, ArgumentArray args, CommandOptions options)
        where T : ICallerContext
    {
        options.Manager = this;

        IExecuteResult? result = null;

        var components = Find(args);

        foreach (var component in components)
        {
            if (component is Command command)
            {
                result = await command.Run(caller, args, options).ConfigureAwait(false);

                if (!result.Success)
                    continue;

                break;
            }

            result ??= new SearchResult(new CommandRouteIncompleteException(component));
        }

        result ??= new SearchResult(new CommandNotFoundException());

        if (_handlersAvailable)
        {
            foreach (var resolver in _handlers)
                await resolver.HandleResult(caller, result, options.Services, options.CancellationToken).ConfigureAwait(false);
        }
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
