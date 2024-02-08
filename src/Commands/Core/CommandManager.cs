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
    /// <remarks>
    /// 
    /// </remarks>
    /// <param name="services"></param>
    /// <param name="logFactory"></param>
    /// <param name="finalizer"></param>
    /// <param name="converters"></param>
    /// <param name="options"></param>
    [DebuggerDisplay("Commands = {Commands}")]
    public class CommandManager(
        IServiceProvider services, ILoggerFactory logFactory, CommandFinalizer finalizer, IEnumerable<TypeConverterBase> converters, BuildOptions options)
    {
        private long scopeid = 0;

        private readonly object s_lock = new();

        private readonly CommandFinalizer _finalizer = finalizer;
        private readonly IServiceProvider _services = services;
        private readonly ILoggerFactory _logFactory = logFactory;

        /// <summary>
        ///     Gets the collection containing all commands, groups and subcommands as implemented by the assemblies that were registered in the <see cref="BuildOptions"/> provided when creating the manager.
        /// </summary>
        public IReadOnlySet<IConditional> Commands { get; } = ReflectionHelpers.BuildComponents(converters, options).ToHashSet();

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
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsyncMode"/> is set to <see cref="AsyncMode.Discard"/>.</returns>
        public virtual async Task TryExecuteAsync<T>(
            T consumer, object[] args, CommandOptions options = default)
            where T : ConsumerBase
        {
            switch (options.AsyncMode)
            {
                case AsyncMode.Await:
                    {
                        await ExecuteAsync(consumer, args, options);
                    }
                    return;
                case AsyncMode.Discard:
                    {
                        _ = ExecuteAsync(consumer, args, options);
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
            lock (s_lock)
            {
                return Commands.SearchMany(args, 0);
            }
        }

        /// <summary>
        ///     Steps through the pipeline in order to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution.</returns>
        protected virtual async Task ExecuteAsync<T>(
            T consumer, object[] args, CommandOptions options)
            where T : ConsumerBase
        {
            IRunResult result = null;

            var searches = Search(args);

            options.Scope ??= _services.CreateAsyncScope();
            options.Logger ??= _logFactory.CreateLogger($"Commands.Request[{Interlocked.Increment(ref scopeid)}]");

            options.Logger.LogDebug("Scope started. Resolved mode: {}", options.AsyncMode);

            foreach (var search in searches.OrderByDescending(x => x.Component.Priority))
            {
                if (search.Component is CommandInfo command)
                {
                    var match = await MatchAsync(consumer, command, search.SearchHeight, args, options);

                    // enter the invocation logic when a match is successful.
                    if (match.Success)
                    {
                        result = await InvokeAsync(consumer, match, options);
                        break;
                    }

                    // We set the failed match if it was not yet set.
                    result ??= match;

                    continue;
                }

                // We set the sub module if it was not yet set.
                result ??= search;

                continue;
            }

            // we set the failed search if it was not yet set.
            result ??= new SearchResult(SearchException.NotFound());

            await _finalizer.FinalizeAsync(consumer, result, options);
        }

        /// <summary>
        ///     Matches the provided <paramref name="command"/> based on the provided <paramref name="args"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="command">A command intended to be matched.</param>
        /// <param name="argHeight">The height at which the command name ends and argument input starts.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the matching process.</returns>
        protected virtual async ValueTask<MatchResult> MatchAsync<T>(
            T consumer, CommandInfo command, int argHeight, object[] args, CommandOptions options)
            where T : ConsumerBase
        {
            // check command preconditions.
            var check = await CheckAsync(consumer, command, options);

            // verify check success, if not, return the failure.
            if (!check.Success)
                return new(command, MatchException.Failed(check.Exception));

            // read the command parameters in right order.
            var readResult = await ConvertAsync(consumer, command, argHeight, args, options);

            // exchange the reads for result, verifying successes in the process.
            var reads = new object[readResult.Length];
            for (int i = 0; i < readResult.Length; i++)
            {
                // check for read success.
                if (!readResult[i].Success)
                    return new(command, readResult[i].Exception);

                reads[i] = readResult[i].Value;
            }

            // return successful match if execution reaches here.
            return new(command, reads);
        }

        /// <summary>
        ///     Evaluates the preconditions of provided <paramref name="command"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="command">The found command intended to be evaluated.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the checking process.</returns>
        protected virtual async ValueTask<ConditionResult> CheckAsync<T>(
            T consumer, CommandInfo command, CommandOptions options)
            where T : ConsumerBase
        {
            if (options.SkipPreconditions)
            {
                options.Logger.LogDebug("Precondition evaluation skipped for {}", command);

                return new(null);
            }

            foreach (var precon in command.Preconditions)
            {
                var checkResult = await precon.EvaluateAsync(consumer, command, options.Scope.ServiceProvider, options.CancellationToken);

                if (!checkResult.Success)
                {
                    options.Logger.LogError("Precondition evaluation failed for {}", command);
                    return checkResult;
                }
            }

            options.Logger.LogInformation("Precondition evaluation succeeded for {}", command);

            return new(null);
        }

        /// <summary>
        ///     Evaluates the postconditions of provided <paramref name="result"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="result">The result of the command intended to be evaluated.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the checking process.</returns>
        protected virtual async ValueTask<ConditionResult> CheckAsync<T>(
            T consumer, InvokeResult result, CommandOptions options)
            where T : ConsumerBase
        {
            if (options.SkipPostconditions)
            {
                options.Logger.LogDebug("Skipped postcondition evaluation for {}", result.Command);
                return new(null);
            }

            foreach (var postcon in result.Command.PostConditions)
            {
                var checkResult = await postcon.EvaluateAsync(consumer, result, options.Scope.ServiceProvider, options.CancellationToken);

                if (!checkResult.Success)
                {
                    options.Logger.LogError("Postcondition evaluation failed for {}", result.Command);
                    return checkResult;
                }
            }

            options.Logger.LogInformation("Postcondition evaluation succeeded for {}", result.Command);

            return new(null);
        }

        /// <summary>
        ///     Converts the provided <paramref name="command"/> based on the provided <paramref name="args"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="command">A command intended to be converted.</param>
        /// <param name="argHeight">The height at which the command name ends and argument input starts.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the results of the conversion process.</returns>
        protected virtual async ValueTask<ConvertResult[]> ConvertAsync<T>(
            T consumer, CommandInfo command, int argHeight, object[] args, CommandOptions options)
            where T : ConsumerBase
        {
            // skip if no parameters exist.
            if (!command.HasArguments)
            {
                options.Logger.LogDebug("Argument evaluation skipped for {}", command);
                return [];
            }

            // determine height of search to discover command name.
            var length = args.Length - argHeight;

            // check if input equals command length.
            if (command.MaxLength == length)
            {
                return await command.Arguments.ConvertManyAsync(consumer, args[^length..], 0, options);
            }

            // check if input is longer than command, but remainder to concatenate.
            if (command.MaxLength <= length && command.HasRemainder)
            {
                return await command.Arguments.ConvertManyAsync(consumer, args[^length..], 0, options);
            }

            // check if input is shorter than command, but optional parameters to replace.
            if (command.MaxLength > length && command.MinLength <= length)
            {
                return await command.Arguments.ConvertManyAsync(consumer, args[^length..], 0, options);
            }

            options.Logger.LogError("Argument evaluation failed for {}", command);

            // check if input is too short.
            if (command.MinLength > length)
            {
                return [new ConvertResult(exception: ConvertException.TooShort())];
            }

            // input is too long.
            return [new ConvertResult(exception: ConvertException.TooLong())];
        }

        /// <summary>
        ///     Invokes the provided <paramref name="match"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="match">The result of the match intended to be ran.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the invocation process.</returns>
        protected virtual async ValueTask<InvokeResult> InvokeAsync<T>(
            T consumer, MatchResult match, CommandOptions options)
            where T : ConsumerBase
        {
            try
            {
                options.Logger.LogDebug("Resolving targets for {}", match.Command);

                var targetInstance = options.Scope.ServiceProvider.GetService(match.Command.Module.Type);

                var module = targetInstance != null
                    ? targetInstance as ModuleBase
                    : ActivatorUtilities.CreateInstance(options.Scope.ServiceProvider, match.Command.Module.Type) as ModuleBase;

                module.Consumer = consumer;
                module.Command = match.Command;

                options.Logger.LogDebug("Starting invocation of {}", match.Command);

                var value = match.Command.Target.Invoke(module, match.Reads);

                var result = await module.ResolveReturnAsync(value);

                options.Logger.LogInformation("Invocation succeeded for {}", match.Command);

                var checkResult = await CheckAsync(consumer, result, options);

                if (!checkResult.Success)
                {
                    return new InvokeResult(match.Command, RunException.Failed(checkResult.Exception));
                }

                return result;
            }
            catch (Exception exception)
            {
                options.Logger.LogError("Invocation failed for {}", match.Command);

                return new(match.Command, exception);
            }
        }
    }
}
