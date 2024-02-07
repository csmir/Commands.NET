using Commands.Exceptions;
using Commands.Helpers;
using Commands.Reflection;
using Microsoft.Extensions.DependencyInjection;

[assembly: CLSCompliant(true)]

namespace Commands.Core
{
    /// <summary>
    ///     The root type serving as a basis for all operations and functionality as provided by the Command Standardization Framework.
    /// </summary>
    /// <remarks>
    ///     To learn more about use of this type and other features of Commands.NET, check out the README on GitHub: <see href="https://github.com/csmir/Commands.NET"/>
    /// </remarks>
    public class CommandManager
    {
        private readonly object _searchLock = new();
        private readonly CommandFinalizer _finalizer;
        private readonly IServiceProvider _services;

        /// <summary>
        ///     Gets the collection containing all commands, groups and subcommands as implemented by the assemblies that were registered in the <see cref="CommandConfiguration"/> provided when creating the manager.
        /// </summary>
        public IReadOnlySet<IConditional> Commands { get; }

        /// <summary>
        ///     Gets the configuration used to configure execution operations and registration options.
        /// </summary>
        public CommandConfiguration Configuration { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="CommandManager"/> with provided arguments.
        /// </summary>
        /// <remarks>
        ///     It is suggested to configure and create the <see cref="CommandManager"/> by calling <see cref="ServiceHelpers.ConfigureCommands(IServiceCollection, Action{CommandConfiguration})"/>.
        ///     Creating the manager manually will have a negative impact on performance, unless each <see cref="ModuleBase"/> is manually added to the <paramref name="services"/> as provided.
        /// </remarks>
        /// <param name="services">A built collection of services that hosts services to be injected or received at request.</param>
        /// <param name="configuration">A configuration to be used to configure the execution and registration of commands.</param>
        public CommandManager(IServiceProvider services, CommandConfiguration configuration)
        {
            if (configuration.Assemblies == null || configuration.Assemblies.Length == 0)
            {
                ThrowHelpers.ThrowInvalidArgument(nameof(configuration.Assemblies));
            }

            Commands = ReflectionHelpers.BuildComponents(configuration)
                .SelectMany(x => x.Components)
                .ToHashSet();

            services ??= ServiceProvider.Default;

            Configuration = configuration;

            _finalizer = services.GetService<CommandFinalizer>() ?? CommandFinalizer.Default;
            _services = services;
        }

        /// <summary>
        ///     Makes an attempt at executing a command from provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases this way also.
        /// </remarks>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        public virtual void TryExecute(ICommandContext context, params object[] args)
            => TryExecuteAsync(context, args).Wait();

        /// <summary>
        ///     Makes an attempt at executing a command from provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases this way also.
        /// </remarks>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandConfiguration.AsyncApproach"/> is set to <see cref="AsyncApproach.Discard"/>.</returns>
        public virtual Task TryExecuteAsync(ICommandContext context, params object[] args)
            => TryExecuteAsync(context, args, cancellationToken: default);

        /// <summary>
        ///     Makes an attempt at executing a command from provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases this way also.
        /// </remarks>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="cancellationToken">A token that can be provided from a <see cref="CancellationTokenSource"/> and later used to cancel asynchronous execution.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandConfiguration.AsyncApproach"/> is set to <see cref="AsyncApproach.Discard"/>.</returns>
        public virtual async Task TryExecuteAsync(ICommandContext context, object[] args, CancellationToken cancellationToken = default)
        {
            switch (Configuration.AsyncApproach)
            {
                case AsyncApproach.Await:
                    {
                        await ExecuteAsync(context, args, cancellationToken);
                    }
                    return;
                case AsyncApproach.Discard:
                    {
                        _ = ExecuteAsync(context, args, cancellationToken);
                    }
                    return;
            }
        }

        /// <summary>
        ///     Searches all commands for any matches of <paramref name="args"/>.
        /// </summary>
        /// <param name="args">A set of arguments intended to discover commands as a query.</param>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> that holds the results of the search query.</returns>
        public virtual IEnumerable<SearchResult> Search(object[] args)
        {
            // recursively search for commands in the execution.
            lock (_searchLock)
            {
                return Commands.RecursiveSearch(args, 0);
            }
        }

        /// <summary>
        ///     Steps through the pipeline in order to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution.</returns>
        protected virtual async Task ExecuteAsync(ICommandContext context, object[] args, CancellationToken cancellationToken)
        {
            var searches = Search(args);

            var scope = _services.CreateAsyncScope();

            var c = 0;

            foreach (var search in searches.OrderByDescending(x => x.Command.Priority))
            {
                c++;

                var match = await MatchAsync(context, scope.ServiceProvider, search, args, cancellationToken);

                // enter the invocation logic when a match is successful.
                if (match.Success())
                {
                    var result = await InvokeAsync(context, scope.ServiceProvider, match, cancellationToken);

                    await _finalizer.FinalizeAsync(context, result, scope, cancellationToken);

                    return;
                }

                context.TrySetFallback(match);
            }

            // if no searches were found, we send searchfailure.
            if (c is 0)
            {
                await _finalizer.FinalizeAsync(context, new SearchResult(new SearchException("No commands were found with the provided input.")), scope, cancellationToken);
                return;
            }

            // if there is a fallback present, we send matchfailure.
            if (context.TryGetFallback(out var fallback))
            {
                await _finalizer.FinalizeAsync(context, fallback, scope, cancellationToken);
            }
        }

        /// <summary>
        ///     Matches the provided <paramref name="search"/> based on the provided <paramref name="args"/> and returns the result.
        /// </summary>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="search"></param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the matching process.</returns>
        protected virtual async ValueTask<MatchResult> MatchAsync(ICommandContext context, IServiceProvider services, SearchResult search, object[] args, CancellationToken cancellationToken)
        {
            // check command preconditions.
            var check = await CheckAsync(context, search, services, cancellationToken);

            // verify check success, if not, return the failure.
            if (!check.Success())
                return new(search.Command, new MatchException("Command failed to reach execution. View inner exception for more details.", check.Exception));

            // read the command parameters in right order.
            var readResult = await ConvertAsync(context, services, search, args, cancellationToken);

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
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="result">The found command intended to be evaluated.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the checking process.</returns>
        protected virtual async ValueTask<ConditionResult> CheckAsync(ICommandContext context, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            foreach (var precon in result.Command.Preconditions)
            {
                var checkResult = await precon.EvaluateAsync(context, result, services, cancellationToken);

                if (!checkResult.Success())
                    return checkResult;
            }

            return new(null);
        }

        /// <summary>
        ///     Evaluates the postconditions of provided <paramref name="result"/> and returns the result.
        /// </summary>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="result">The result of the command intended to be evaluated.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the checking process.</returns>
        protected virtual async ValueTask<ConditionResult> CheckAsync(ICommandContext context, InvokeResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            foreach (var postcon in result.Command.PostConditions)
            {
                var checkResult = await postcon.EvaluateAsync(context, result, services, cancellationToken);

                if (!checkResult.Success())
                    return checkResult;
            }

            return new(null);
        }

        /// <summary>
        ///     Converts the provided <paramref name="search"/> based on the provided <paramref name="args"/> and returns the result.
        /// </summary>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="search">The result of the search intended to be converted.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the results of the conversion process.</returns>
        protected virtual async ValueTask<ConvertResult[]> ConvertAsync(ICommandContext context, IServiceProvider services, SearchResult search, object[] args, CancellationToken cancellationToken)
        {
            // skip if no parameters exist.
            if (!search.Command.HasArguments)
                return [];

            // determine height of search to discover command name.
            var length = args.Length - search.SearchHeight;

            // check if input equals command length.
            if (search.Command.MaxLength == length)
                return await search.Command.Arguments.RecursiveConvertAsync(context, services, args[^length..], 0, cancellationToken);

            // check if input is longer than command, but remainder to concatenate.
            if (search.Command.MaxLength <= length && search.Command.HasRemainder)
                return await search.Command.Arguments.RecursiveConvertAsync(context, services, args[^length..], 0, cancellationToken);

            // check if input is shorter than command, but optional parameters to replace.
            if (search.Command.MaxLength > length && search.Command.MinLength <= length)
                return await search.Command.Arguments.RecursiveConvertAsync(context, services, args[^length..], 0, cancellationToken);

            // input is too long or too short.
            return [];
        }

        /// <summary>
        ///     Invokes the provided <paramref name="match"/> and returns the result.
        /// </summary>
        /// <param name="context">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="services">The scoped <see cref="IServiceProvider"/> used for executing this command.</param>
        /// <param name="match">The result of the match intended to be ran.</param>
        /// <param name="cancellationToken">The token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the invocation process.</returns>
        protected virtual async ValueTask<InvokeResult> InvokeAsync(ICommandContext context, IServiceProvider services, MatchResult match, CancellationToken cancellationToken)
        {
            try
            {
                var targetInstance = services.GetService(match.Command.Module.Type);

                var module = targetInstance != null
                    ? targetInstance as ModuleBase
                    : ActivatorUtilities.CreateInstance(services, match.Command.Module.Type) as ModuleBase;

                module.Context = context;
                module.Command = match.Command;

                var value = match.Command.Target.Invoke(module, match.Reads);

                var result = await module.ResolveReturnAsync(value);

                var checkResult = await CheckAsync(context, result, services, cancellationToken);

                if (!checkResult.Success())
                {
                    return new InvokeResult(match.Command, new RunException("Command failed to finalize execution. View inner exception for more details.", checkResult.Exception));
                }

                return result;
            }
            catch (Exception exception)
            {
                return new(match.Command, exception);
            }
        }

        internal class ServiceProvider : IServiceProvider
        {
            private static readonly Lazy<ServiceProvider> _i = new();

            public object GetService(Type serviceType)
            {
                return null;
            }

            public static IServiceProvider Default
            {
                get
                {
                    return _i.Value;
                }
            }
        }
    }
}
