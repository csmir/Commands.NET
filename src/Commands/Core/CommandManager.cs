using Commands.Exceptions;
using Commands.Helpers;
using Commands.Reflection;
using Commands.TypeConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

[assembly: CLSCompliant(true)]

namespace Commands.Core
{
    /// <summary>
    ///     The root type serving as a basis for all operations and functionality as provided by Commands.NET.
    /// </summary>
    /// <remarks>
    ///     To learn more about use of this type and other features of Commands.NET, check out the README on GitHub: <see href="https://github.com/csmir/Commands.NET"/>
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="logFactory"></param>
    /// <param name="finalizer"></param>
    /// <param name="converters"></param>
    /// <param name="context"></param>
    [DebuggerDisplay("Commands = {Commands}")]
    public class CommandManager(
        IServiceProvider services,
        ILoggerFactory logFactory,
        CommandFinalizer finalizer,
        IEnumerable<TypeConverterBase> converters,
        BuildingContext context)
    {
        private long _scopeId = 0;

        private readonly object _searchLock = new();
        private readonly CommandFinalizer _finalizer = finalizer;
        private readonly IServiceProvider _services = services;
        private readonly ILoggerFactory _logFactory = logFactory;

        /// <summary>
        ///     Gets the collection containing all commands, groups and subcommands as implemented by the assemblies that were registered in the <see cref="BuildingContext"/> provided when creating the manager.
        /// </summary>
        public IReadOnlySet<IConditional> Commands { get; } = Build(converters, context);

        /// <summary>
        ///     Makes an attempt at executing a command from provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases this way also.
        /// </remarks>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        public virtual void TryExecute<T>(T context, params object[] args)
            where T : ConsumerBase
            => TryExecuteAsync(context, args).Wait();

        /// <summary>
        ///     Makes an attempt at executing a command from provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases this way also.
        /// </remarks>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="context">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="RequestContext.AsyncApproach"/> is set to <see cref="AsyncApproach.Discard"/>.</returns>
        public virtual async Task TryExecuteAsync<T>(T consumer, object[] args, RequestContext context = default)
            where T : ConsumerBase
        {
            switch (context.AsyncApproach)
            {
                case AsyncApproach.Await:
                    {
                        await ExecuteAsync(consumer, args, context);
                    }
                    return;
                case AsyncApproach.Discard:
                    {
                        _ = ExecuteAsync(consumer, args, context);
                    }
                    return;
            }
        }

        /// <summary>
        ///     Runs a thread safe search operation over all commands for any matches of <paramref name="args"/>.
        /// </summary>
        /// <param name="args">A set of arguments intended to discover commands as a query.</param>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> that holds the results of the search query.</returns>
        public virtual IEnumerable<SearchResult> Search(object[] args)
        {
            // dont allow args to be null.
            if (args == null)
            {
                ThrowHelpers.ThrowInvalidArgument(args);
            }

            // return empty collection for empty argument collection.
            if (args.Length == 0)
            {
                return [];
            }

            // recursively search for commands in the execution.
            lock (_searchLock)
            {
                return Commands.RecursiveSearch(args, 0);
            }
        }

        /// <summary>
        ///     Steps through the pipeline in order to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="context">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution.</returns>
        protected virtual async Task ExecuteAsync<T>(T consumer, object[] args, RequestContext context)
            where T : ConsumerBase
        {
            var searches = Search(args);

            var scope = _services.CreateAsyncScope();
            context.Logger ??= _logFactory.CreateLogger($"Commands.Request[{Interlocked.Increment(ref _scopeId)}]");

            context.Logger.LogDebug("Resolved workflow: {}", context.AsyncApproach);

            var c = 0;

            foreach (var search in searches.OrderByDescending(x => x.Command.Priority))
            {
                c++;

                var match = await MatchAsync(consumer, scope.ServiceProvider, search, args, context);

                // enter the invocation logic when a match is successful.
                if (match.Success())
                {
                    var result = await InvokeAsync(consumer, scope.ServiceProvider, match, context);

                    await _finalizer.FinalizeAsync(consumer, result, scope, context);

                    return;
                }

                consumer.TrySetFallback(match);
            }

            // if no searches were found, we send searchfailure.
            if (c is 0)
            {
                context.Logger.LogError("No commands found.");

                await _finalizer.FinalizeAsync(consumer, new SearchResult(new SearchException("No commands were found with the provided input.")), scope, context);
                return;
            }

            // if there is a fallback present, we send matchfailure.
            if (consumer.TryGetFallback(out var fallback))
            {
                await _finalizer.FinalizeAsync(consumer, fallback, scope, context);
            }
        }

        /// <summary>
        ///     Matches the provided <paramref name="search"/> based on the provided <paramref name="args"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="search"></param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="context">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the matching process.</returns>
        protected virtual async ValueTask<MatchResult> MatchAsync<T>(T consumer, IServiceProvider services, SearchResult search, object[] args, RequestContext context)
            where T : ConsumerBase
        {
            context.Logger.LogDebug("Match process starting on search {}", search.Command);

            // check command preconditions.
            var check = await CheckAsync(consumer, search, services, context);

            // verify check success, if not, return the failure.
            if (!check.Success())
                return new(search.Command, new MatchException("Command failed to reach execution. View inner exception for more details.", check.Exception));

            // read the command parameters in right order.
            var readResult = await ConvertAsync(consumer, services, search, args, context);

            // exchange the reads for result, verifying successes in the process.
            var reads = new object[readResult.Length];
            for (int i = 0; i < readResult.Length; i++)
            {
                // check for read success.
                if (!readResult[i].Success())
                    return new(search.Command, readResult[i].Exception);

                reads[i] = readResult[i].Value;
            }

            // return successful match if execution reaches here.
            return new(search.Command, reads);
        }

        /// <summary>
        ///     Evaluates the preconditions of provided <paramref name="result"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="result">The found command intended to be evaluated.</param>
        /// <param name="context">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the checking process.</returns>
        protected virtual async ValueTask<ConditionResult> CheckAsync<T>(T consumer, SearchResult result, IServiceProvider services, RequestContext context)
            where T : ConsumerBase
        {
            if (context.SkipPreconditions)
            {
                context.Logger.LogDebug("Precondition evaluation skipped for {}", result.Command);

                return new(null);
            }

            context.Logger.LogDebug("Precondition evaluation starting for {}", result.Command);

            foreach (var precon in result.Command.Preconditions)
            {
                var checkResult = await precon.EvaluateAsync(consumer, result, services, context.CancellationToken);

                if (!checkResult.Success())
                {
                    context.Logger.LogError("Precondition evaluation failed for {}", result.Command);
                    return checkResult;
                }
            }

            context.Logger.LogInformation("Precondition evaluation succeeded for {}", result.Command);

            return new(null);
        }

        /// <summary>
        ///     Evaluates the postconditions of provided <paramref name="result"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="result">The result of the command intended to be evaluated.</param>
        /// <param name="context">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the checking process.</returns>
        protected virtual async ValueTask<ConditionResult> CheckAsync<T>(T consumer, InvokeResult result, IServiceProvider services, RequestContext context)
            where T : ConsumerBase
        {
            if (context.SkipPostconditions)
            {
                context.Logger.LogDebug("Skipped postcondition evaluation for {}", result.Command);
                return new(null);
            }

            context.Logger.LogDebug("Postcondition evaluation starting for {}", result.Command);

            foreach (var postcon in result.Command.PostConditions)
            {
                var checkResult = await postcon.EvaluateAsync(consumer, result, services, context.CancellationToken);

                if (!checkResult.Success())
                {
                    context.Logger.LogError("Postcondition evaluation failed for {}", result.Command);
                    return checkResult;
                }
            }

            context.Logger.LogInformation("Postcondition evaluation succeeded for {}", result.Command);

            return new(null);
        }

        /// <summary>
        ///     Converts the provided <paramref name="search"/> based on the provided <paramref name="args"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="search">The result of the search intended to be converted.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="context">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the results of the conversion process.</returns>
        protected virtual async ValueTask<ConvertResult[]> ConvertAsync<T>(T consumer, IServiceProvider services, SearchResult search, object[] args, RequestContext context)
            where T : ConsumerBase
        {
            // skip if no parameters exist.
            if (!search.Command.HasArguments)
            {
                context.Logger.LogDebug("Argument evaluation skipped for {}", search.Command);
                return [];
            }

            context.Logger.LogDebug("Argument evaluation starting for {}", search.Command);

            // determine height of search to discover command name.
            var length = args.Length - search.SearchHeight;

            // check if input equals command length.
            if (search.Command.MaxLength == length)
            {
                return await search.Command.Arguments.RecursiveConvertAsync(consumer, services, args[^length..], 0, context);
            }

            // check if input is longer than command, but remainder to concatenate.
            if (search.Command.MaxLength <= length && search.Command.HasRemainder)
            {
                return await search.Command.Arguments.RecursiveConvertAsync(consumer, services, args[^length..], 0, context);
            }

            // check if input is shorter than command, but optional parameters to replace.
            if (search.Command.MaxLength > length && search.Command.MinLength <= length)
            {
                return await search.Command.Arguments.RecursiveConvertAsync(consumer, services, args[^length..], 0, context);
            }

            context.Logger.LogError("Argument evaluation failed for {}", search.Command);

            // check if input is too short.
            if (search.Command.MinLength > length)
            {
                return [new ConvertResult(exception: new ConvertException("Query is too short for best match."))];
            }

            // input is too long.
            return [new ConvertResult(exception: new ConvertException("Query is too long for best match."))];
        }

        /// <summary>
        ///     Invokes the provided <paramref name="match"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="match">The result of the match intended to be ran.</param>
        /// <param name="context">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the invocation process.</returns>
        protected virtual async ValueTask<InvokeResult> InvokeAsync<T>(T consumer, IServiceProvider services, MatchResult match, RequestContext context)
            where T : ConsumerBase
        {
            try
            {
                context.Logger.LogDebug("Resolving targets for {}", match.Command);

                var targetInstance = services.GetService(match.Command.Module.Type);

                var module = targetInstance != null
                    ? targetInstance as ModuleBase
                    : ActivatorUtilities.CreateInstance(services, match.Command.Module.Type) as ModuleBase;

                module.Consumer = consumer;
                module.Command = match.Command;

                context.Logger.LogDebug("Starting invocation of {}", match.Command);

                var value = match.Command.Target.Invoke(module, match.Reads);

                var result = await module.ResolveReturnAsync(value);

                context.Logger.LogInformation("Invocation succeeded for {}", match.Command);

                var checkResult = await CheckAsync(consumer, result, services, context);

                if (!checkResult.Success())
                {
                    return new InvokeResult(match.Command, new RunException("Command failed to finalize execution. View inner exception for more details.", checkResult.Exception));
                }

                return result;
            }
            catch (Exception exception)
            {
                context.Logger.LogError("Invocation failed for {}", match.Command);

                return new(match.Command, exception);
            }
        }

        private static HashSet<IConditional> Build(IEnumerable<TypeConverterBase> converters, BuildingContext context)
            => ReflectionHelpers.BuildComponents(converters, context)
                .SelectMany(x => x.Components)
                .ToHashSet();
    }
}
