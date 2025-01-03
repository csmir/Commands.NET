
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;

using Commands.Builders;

namespace Commands;

/// <summary>
///     A concurrent implementation of the mechanism that allows commands to be executed using a provided set of arguments. This class cannot be inherited.
/// </summary>
/// <inheritdoc cref="IComponentTree"/>
[DebuggerDisplay("Count = {Count}")]
public sealed class ComponentTree : ComponentCollection, IComponentTree
{
    private bool _handlersAvailable;

    private readonly ResultHandler[] _handlers;

    internal ComponentTree(IEnumerable<IComponent> components, ResultHandler[] handlers)
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

        _handlersAvailable = handlers.Length > 0;
        _handlers = handlers;
    }

    /// <inheritdoc />
    public override IEnumerable<SearchResult> Find(ArgumentArray args)
    {
        List<SearchResult> discovered = [];

        var index = 0;

        foreach (var component in this)
        {
            if (!args.TryGetElementAt(index, out var value) || !component.Names.Contains(value))
                continue;

            if (component is CommandGroup group)
                discovered.AddRange(group.Find(args));
            else
                discovered.Add(SearchResult.FromSuccess(component, index + 1));
        }

        return discovered;
    }

    /// <inheritdoc />
    public void Execute<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext
        => Execute(caller, ArgumentArray.Read(args), options);

    /// <inheritdoc />
    public void Execute<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext
        => Execute(caller, new ArgumentArray(args), options ?? new CommandOptions());

    /// <inheritdoc />
    public void Execute<T>(T caller, KeyValuePair<string, object?>[] args, CommandOptions? options = null)
        where T : ICallerContext
        => Execute(caller, new ArgumentArray(args, StringComparer.OrdinalIgnoreCase), options);

    /// <inheritdoc />
    public void Execute<T>(T caller, ArgumentArray args, CommandOptions? options = null)
        where T : ICallerContext
        => StartAsynchronousPipeline(caller, args, options ?? new()).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task ExecuteAsync<T>(T caller, string? args, CommandOptions? options = null)
        where T : ICallerContext
        => ExecuteAsync(caller, ArgumentArray.Read(args), options);

    /// <inheritdoc />
    public Task ExecuteAsync<T>(T caller, string[] args, CommandOptions? options = null)
        where T : ICallerContext
        => ExecuteAsync(caller, new ArgumentArray(args), options ?? new CommandOptions());

    /// <inheritdoc />
    public Task ExecuteAsync<T>(T caller, KeyValuePair<string, object?>[] args, CommandOptions? options = null)
        where T : ICallerContext
        => ExecuteAsync(caller, new ArgumentArray(args, StringComparer.OrdinalIgnoreCase), options);

    /// <inheritdoc />
    public Task ExecuteAsync<T>(T caller, ArgumentArray args, CommandOptions? options = null)
        where T : ICallerContext
    {
        options ??= new CommandOptions();

        var task = StartAsynchronousPipeline(caller, args, options);

        if (options.AsynchronousExecution)
            return Task.CompletedTask;

        return task;
    }

    private async Task StartAsynchronousPipeline<T>(T caller, ArgumentArray args, CommandOptions options)
        where T : ICallerContext
    {
        IExecuteResult? result = null;

        var searches = Find(args);
        foreach (var search in searches)
        {
            if (search.Component is Command command)
            {
                result = await InvokeCommand(caller, command, search.SearchHeight, args, options);

                if (!result.Success)
                    continue;

                break;
            }

            result ??= search;
            continue;
        }

        result ??= SearchResult.FromError();

        if (_handlersAvailable)
        {
            foreach (var resolver in _handlers)
                await resolver.HandleResult(caller, result, options.Services, options.CancellationToken);
        }
    }

    private async ValueTask<IExecuteResult> InvokeCommand<T>(T caller, Command command, int parseIndex, ArgumentArray args, CommandOptions options)
        where T : ICallerContext
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        var conversion = await ParseCommand(caller, command, parseIndex, args, options);

        var arguments = new object?[conversion.Length];

        for (int i = 0; i < conversion.Length; i++)
        {
            if (!conversion[i].Success)
                return MatchResult.FromError(command, conversion[i].Exception!);

            arguments[i] = conversion[i].Value;
        }

        if (!options.SkipConditions)
        {
            foreach (var condition in command.Evaluators)
            {
                var checkResult = await condition.Evaluate(caller, command, options.Services, options.CancellationToken);

                if (!checkResult.Success)
                    return checkResult;
            }
        }

        try
        {
            var value = command.Activator.Invoke(caller, command, arguments, this, options);

            return InvokeResult.FromSuccess(command, value);
        }
        catch (Exception exception)
        {
            return InvokeResult.FromError(command, exception);
        }
    }

    private async ValueTask<ParseResult[]> ParseCommand<T>(T caller, Command command, int parseIndex, ArgumentArray args, CommandOptions options)
        where T : ICallerContext
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        args.SetParseIndex(parseIndex);

        if (!command.HasParameters && args.Length == 0)
            return [];

        if (command.MaxLength == args.Length)
            return await ParseParameters(caller, command.Parameters, args, options);

        if (command.MaxLength <= args.Length && command.HasRemainder)
            return await ParseParameters(caller, command.Parameters, args, options);

        if (command.MaxLength > args.Length && command.MinLength <= args.Length)
            return await ParseParameters(caller, command.Parameters, args, options);

        return [ParseResult.FromError(ParseException.ArgumentMismatch(command.MinLength, args.Length))];
    }

    private async ValueTask<ParseResult[]> ParseParameters(ICallerContext caller, ICommandParameter[] parameters, ArgumentArray args, CommandOptions options)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        var results = new ParseResult[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var argument = parameters[i];

            if (argument.IsRemainder)
            {
                results[i] = await argument.Parse(caller, argument.IsCollection ? args.TakeRemaining() : args.TakeRemaining(options.RemainderSeparator), options.Services, options.CancellationToken);
                break;
            }

            if (argument is CommandComplexParameter complexParameter)
            {
                var result = await ParseParameters(caller, complexParameter.Parameters, args, options);

                if (result.All(x => x.Success))
                {
                    try
                    {
                        results[i] = ParseResult.FromSuccess(complexParameter.Activator.Invoke(caller, null, result.Select(x => x.Value).ToArray(), null, options));
                    }
                    catch (Exception ex)
                    {
                        results[i] = ParseResult.FromError(ex);
                    }

                    continue;
                }

                if (complexParameter.IsOptional)
                    results[i] = ParseResult.FromSuccess(Type.Missing);

                continue;
            }

            if (args.TryGetElement(argument.Name!, out var value))
                results[i] = await argument.Parse(caller, value, options.Services, options.CancellationToken);
            else if (argument.IsOptional)
                results[i] = ParseResult.FromSuccess(Type.Missing);
            else
                results[i] = ParseResult.FromError(new ArgumentNullException(argument.Name));
        }

        return results;
    }

    /// <summary>
    ///     Creates a new empty instance of a <see cref="ComponentTree"/> with a default result handler.
    /// </summary>
    /// <returns>A newly created instance of <see cref="ComponentTree"/> which is able to discover and execute components based on provided input.</returns>
    public static IComponentTree Create()
        => Create([]);

    /// <summary>
    ///     Creates a new instance of a <see cref="ComponentTree"/> with a default result handler.
    /// </summary>
    /// <param name="components">A collection of top-level components that should be prepared for execution by the created tree.</param>
    /// <returns>A newly created instance of <see cref="ComponentTree"/> which is able to discover and execute components based on provided input.</returns>
    public static IComponentTree Create(params IComponent[] components)
    {
        var basicResultHandler = new DelegateResultHandler(async (ctx, res, serv) =>
        {
            if (ctx is AsyncCallerContext asyncCtx)
                await asyncCtx.Respond(res);
            else
                ctx.Respond(res);
        });

        return Create(components, [basicResultHandler]);
    }

    /// <summary>
    ///     Creates a new instance of a <see cref="ComponentTree"/> with the provided result handlers.
    /// </summary>
    /// <param name="components">A collection of top-level components that should be prepared for execution by the created tree.</param>
    /// <param name="resultHandlers">A collection of result handlers, responsible for resolving execution results.</param>
    /// <returns>A newly created instance of <see cref="ComponentTree"/> which is able to discover and execute components based on provided input.</returns>
    public static IComponentTree Create(IEnumerable<IComponent> components, ResultHandler[] resultHandlers)
        => new ComponentTree(components, resultHandlers);

    /// <summary>
    ///     Creates a builder that is responsible for setting up all required variables to create, search and run commands from a <see cref="ComponentTree"/>. This builder is pre-configured with default settings.
    /// </summary>
    /// <returns>A new instance of <see cref="ComponentTreeBuilder"/> that builds into a new instance of the <see cref="ComponentTree"/>.</returns>
    public static ITreeBuilder CreateBuilder()
        => new ComponentTreeBuilder();
}
