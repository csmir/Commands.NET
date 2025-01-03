
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
    private readonly ResultHandler[] _handlers;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComponentTree"/> class with a standard result handler that sends faulty results back to the <see cref="ICallerContext"/>.
    /// </summary>
    /// <param name="components">A collection of executable components that can be executed by calling <see cref="ExecuteAsync{T}(T, string, CommandOptions?)"/> or any overload of the same method.</param>
    public ComponentTree(IEnumerable<IComponent> components)
        : this(components, new DelegateResultHandler(async (ctx, res, serv) =>
        {
            if (ctx is IAsyncCallerContext asyncCtx)
                await asyncCtx.Respond(res);
            else
                ctx.Respond(res);
        })) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ComponentTree"/> class, which serves as a container for executing commands.
    /// </summary>
    /// <param name="components">A collection of executable components that can be executed by calling <see cref="ExecuteAsync{T}(T, string, CommandOptions?)"/> or any overload of the same method.</param>
    /// <param name="handlers">A collection of <see cref="ResultHandler"/> implementations that handle results returned by the handler.</param>
    public ComponentTree(IEnumerable<IComponent> components, params ResultHandler[] handlers)
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

        Push(topLevelComponents.OrderByDescending(x => x.Score));

        _handlers = handlers;
    }

    /// <inheritdoc />
    public override IEnumerable<SearchResult> Find(ArgumentEnumerator args)
    {
        List<SearchResult> discovered = [];

        var searchHeight = 0;

        foreach (var component in this)
        {
            if (!args.TryNext(searchHeight, out var value) || !component.Aliases.Contains(value))
                continue;

            if (component is CommandGroup group)
                discovered.AddRange(group.Find(args));
            else
                discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
        }

        return discovered;
    }

    /// <inheritdoc />
    public void Execute<T>(T caller, string args, CommandOptions? options = null) 
        where T : ICallerContext
#if NET8_0_OR_GREATER
        => Execute(caller, ArgumentReader.ReadNamed(args), options);
#else
        => Execute(caller, ArgumentReader.Read(args), options);
#endif

    /// <inheritdoc />
    public void Execute<T>(T caller, IEnumerable<object> args, CommandOptions? options = null) 
        where T : ICallerContext
        => Execute(caller, new ArgumentEnumerator(args), options ?? new CommandOptions());

    /// <inheritdoc />
    public void Execute<T>(T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null) 
        where T : ICallerContext
    {
        options ??= new CommandOptions();

        Execute(caller, new ArgumentEnumerator(args, options.Comparer), options);
    }

    /// <inheritdoc />
    public void Execute<T>(T caller, ArgumentEnumerator args, CommandOptions options) 
        where T : ICallerContext
        => StartAsynchronousPipeline(caller, args, options).GetAwaiter().GetResult();

    /// <inheritdoc />
    public Task ExecuteAsync<T>(
        T caller, string args, CommandOptions? options = null)
        where T : ICallerContext
#if NET8_0_OR_GREATER
        => ExecuteAsync(caller, ArgumentReader.ReadNamed(args), options);
#else
        => ExecuteAsync(caller, ArgumentReader.Read(args), options);
#endif

    /// <inheritdoc />
    public Task ExecuteAsync<T>(
        T caller, IEnumerable<object> args, CommandOptions? options = null)
        where T : ICallerContext
        => ExecuteAsync(caller, new ArgumentEnumerator(args), options ?? new CommandOptions());

    /// <inheritdoc />
    public Task ExecuteAsync<T>(
        T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
        where T : ICallerContext
    {
        options ??= new CommandOptions();

        return ExecuteAsync(caller, new ArgumentEnumerator(args, options.Comparer), options);
    }

    /// <inheritdoc />
    public Task ExecuteAsync<T>(
        T caller, ArgumentEnumerator args, CommandOptions options)
        where T : ICallerContext
    {
        var task = StartAsynchronousPipeline(caller, args, options);

        if (options.AsynchronousExecution)
            return Task.CompletedTask;

        return task;
    }

    private async Task StartAsynchronousPipeline<T>(
        T caller, ArgumentEnumerator args, CommandOptions options)
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

        foreach (var resolver in _handlers)
            await resolver.HandleResult(caller, result, options.Services, options.CancellationToken);
    }

    private async ValueTask<IExecuteResult> InvokeCommand<T>(
        T caller, Command command, int argHeight, ArgumentEnumerator args, CommandOptions options)
        where T : ICallerContext
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        var conversion = await ParseCommand(caller, command, argHeight, args, options);

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

    private async ValueTask<ParseResult[]> ParseCommand<T>(
        T caller, Command command, int argHeight, ArgumentEnumerator args, CommandOptions options)
        where T : ICallerContext
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        args.SetSize(argHeight);

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

    private async ValueTask<ParseResult[]> ParseParameters(
        ICallerContext caller, ICommandParameter[] parameters, ArgumentEnumerator args, CommandOptions options)
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

            if (args.TryNext(argument.Name!, out var value))
                results[i] = await argument.Parse(caller, value, options.Services, options.CancellationToken);
            else if (argument.IsOptional)
                results[i] = ParseResult.FromSuccess(Type.Missing);
            else
                results[i] = ParseResult.FromError(new ArgumentNullException(argument.Name));
        }

        return results;
    }

    /// <summary>
    ///     Creates a builder that is responsible for setting up all required variables to create, search and run commands from a <see cref="ComponentTree"/>. This builder is pre-configured with default settings.
    /// </summary>
    /// <returns>A new instance of <see cref="ComponentTreeBuilder"/> that builds into a new instance of the <see cref="ComponentTree"/>.</returns>
    public static ITreeBuilder CreateBuilder()
        => new ComponentTreeBuilder();
}
