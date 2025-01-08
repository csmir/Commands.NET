﻿using Commands.Builders;
using Commands.Parsing;

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
        : this([], handlers) { }

    internal ComponentManager(IEnumerable<IComponent> components, IEnumerable<ResultHandler> handlers)
        : base(false)
    {
        var topLevelComponents = new List<IComponent>();

        foreach (var component in components)
        {
            if (component.IsSearchable)
                topLevelComponents.Add(component);

            else if (component is ComponentCollection collection)
            {
                collection.Bind(this);

                topLevelComponents.AddRange(collection);
            }
        }

        Push(topLevelComponents.OrderByDescending(x => x.GetScore()));

        var arr = handlers.ToArray();

        _handlersAvailable = arr.Length > 0;
        _handlers = arr;
    }

    /// <inheritdoc />
    public override IEnumerable<KeyValuePair<int, IComponent>> Find(ArgumentArray args)
    {
        List<KeyValuePair<int, IComponent>> discovered = [];

        foreach (var component in this)
        {
            if (!args.TryGetElementAt(0, out var value) || !component.Names.Contains(value))
                continue;

            if (component is CommandGroup group)
                discovered.AddRange(group.Find(args));
            else
                discovered.Add(new(1, component));
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

        var searches = Find(args);

        foreach (var search in searches)
        {
            if (search.Value is Command command)
            {
                // Reset the result if we're going to attempt to run a new command. We only output the last occurred error.
                result = null;

                var conversion = await command.Parse(caller, search.Key, args, options);

                var arguments = new object?[conversion.Length];

                for (int i = 0; i < conversion.Length; i++)
                {
                    if (!conversion[i].Success)
                        result ??= ParseResult.FromError(new CommandParsingException(command, conversion[i].Exception));

                    arguments[i] = conversion[i].Value;
                }

                result ??= await command.Run(caller, arguments, options);

                if (!result.Success)
                    continue;

                break;
            }

            result ??= new SearchResult(new CommandRouteIncompleteException(search.Value));
        }

        result ??= new SearchResult(new CommandNotFoundException());

        if (_handlersAvailable)
        {
            foreach (var resolver in _handlers)
                await resolver.HandleResult(caller, result, options.Services, options.CancellationToken);
        }
    }

    /// <summary>
    ///     Creates a new empty instance of a <see cref="ComponentManager"/> with a default result handler.
    /// </summary>
    /// <returns>A newly created instance of <see cref="ComponentManager"/> which is able to discover and execute components based on provided input.</returns>
    public static ComponentManager Create()
        => Create([]);

    /// <summary>
    ///     Creates a new instance of a <see cref="ComponentManager"/> with a default result handler.
    /// </summary>
    /// <param name="components">A collection of top-level components that should be prepared for execution by the created manager.</param>
    /// <returns>A newly created instance of <see cref="ComponentManager"/> which is able to discover and execute components based on provided input.</returns>
    public static ComponentManager Create(params IComponent[] components)
    {
        var basicResultHandler = new DelegateResultHandler<ICallerContext>(async (ctx, res, serv) =>
        {
            if (ctx is AsyncCallerContext asyncCtx)
                await asyncCtx.Respond(res);
            else
                ctx.Respond(res);
        });

        return Create(components, [basicResultHandler]);
    }

    /// <summary>
    ///     Creates a new instance of a <see cref="ComponentManager"/> with the provided result handlers.
    /// </summary>
    /// <param name="components">A collection of top-level components that should be prepared for execution by the created manager.</param>
    /// <param name="resultHandlers">A collection of result handlers, responsible for resolving execution results.</param>
    /// <returns>A newly created instance of <see cref="ComponentManager"/> which is able to discover and execute components based on provided input.</returns>
    public static ComponentManager Create(IEnumerable<IComponent> components, ResultHandler[] resultHandlers)
        => new(components, resultHandlers);

    /// <summary>
    ///     Creates a builder that is responsible for setting up all required variables to create, search and run commands from a <see cref="ComponentManager"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="ComponentManagerBuilder"/> that builds into a new instance of the <see cref="ComponentManager"/>.</returns>
    public static IManagerBuilder CreateBuilder()
        => new ComponentManagerBuilder();
}
