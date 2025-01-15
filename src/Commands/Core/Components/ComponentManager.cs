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
    ///     Initializes a new instance of the <see cref="ComponentManager"/> class.
    /// </summary>
    /// <remarks>
    ///     This constructor is dependency-injection friendly, allowing <see cref="ResultHandler"/> service implementations to be injected from an <see cref="IServiceProvider"/>.
    ///     To add components to this manager, approach it as an <see cref="ICollection{T}"/>.
    /// </remarks>
    /// <param name="handlers">A collection of <see cref="ResultHandler"/> implementations that should resolve command results by defined approaches.</param>
    public ComponentManager(IEnumerable<ResultHandler> handlers)
        : base(false)
    {
        var arr = handlers.ToArray();

        _handlersAvailable = arr.Length > 0;
        _handlers = arr;
    }

    /// <inheritdoc />
    public override IEnumerable<IComponent> Find(ArgumentArray args)
    {
        List<IComponent> discovered = [];

        foreach (var component in this)
        {
            if (!args.TryGetElementAt(0, out var value) || !component.Names.Contains(value))
                continue;

            if (component is CommandGroup group)
                discovered.AddRange(group.Find(args));
            else
                discovered.Add(component);
        }

        return discovered;
    }

    /// <inheritdoc />
    public void TryExecute<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecute(caller, ArgumentArray.Read(args), options);

    /// <inheritdoc />
    public void TryExecute<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, ArgumentArray.Read(args), options).Wait();

    /// <inheritdoc />
    public void TryExecute<T>(T caller, ArgumentArray args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, args, options).Wait();

    /// <inheritdoc />
    public Task TryExecuteAsync<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, ArgumentArray.Read(args), options);

    /// <inheritdoc />
    public Task TryExecuteAsync<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext
        => TryExecuteAsync(caller, ArgumentArray.Read(args), options);

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

    public static ComponentManagerProperties From()
        => new();
}
