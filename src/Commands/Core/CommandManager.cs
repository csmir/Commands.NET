using Commands.Helpers;
using Commands.Parsing;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.Converters;
using System.Diagnostics;
using System.Collections;

[assembly: CLSCompliant(true)]

namespace Commands
{
    /// <summary>
    ///     The root type serving as a basis for all operations and functionality as provided by Commands.NET. 
    ///     To learn more about use of this type and other features of Commands.NET, check out the README on GitHub: <see href="https://github.com/csmir/Commands.NET"/>
    /// </summary>
    /// <remarks>
    ///     This type can be implemented to override the virtual execution steps. A hosted implementation exists in the <b>Commands.NET.Hosting</b> package,
    ///     which introduces native IoC based command operations alongside the preexisting DI support.
    ///     <br/>
    ///     <br/>
    ///     To start using this manager, call <see cref="CreateDefaultBuilder"/> and configure it using the minimal API's implemented by the <see cref="ConfigurationBuilder"/>.
    /// </remarks>
    [DebuggerDisplay("Commands = {Commands},nq")]
    public sealed class CommandManager
    {
        private readonly SequenceDisposer _disposer;

        /// <summary>
        ///     Gets the collection containing all commands, named modules and subcommands as implem ented by the assemblies that were registered in the <see cref="ConfigurationBuilder"/> provided when creating the manager.
        /// </summary>
        public HashSet<ISearchable> Commands { get; }

        /// <summary>
        ///     Gets the configuration that was used to construct the <see cref="CommandManager"/>, and which can be used in adding extended functionality to the manager.
        /// </summary>
        public CommandConfiguration Configuration { get; }

        /// <summary>
        ///     Creates a new <see cref="CommandManager"/> based on the provided arguments.
        /// </summary>
        /// <param name="configuration">The options through which to construct the collection of <see cref="Commands"/>.</param>
        public CommandManager(CommandConfiguration configuration)
        {
            _disposer = new SequenceDisposer(configuration.ResultResolvers);

            var commands = ReflectionUtilities.GetTopLevelComponents(configuration)
                .Concat(configuration.Commands.Select(x => x.Build(configuration)))
                .OrderByDescending(command => command.Score);

            Commands = [.. commands];
            Configuration = configuration;
        }

        /// <summary>
        ///     Flattens the top level commands in <see cref="Commands"/> into a single enumerable collection.
        /// </summary>
        /// <returns>A lazily evaluated, flattened <see cref="IEnumerable{T}"/> that holds all <see cref="CommandInfo"/>'s registered in this manager.</returns>
        public IEnumerable<CommandInfo> GetCommands()
        {
            static IEnumerable<CommandInfo> Flatten(IEnumerable<ISearchable> components)
            {
                foreach (var item in components)
                {
                    if (item is CommandInfo command)
                    {
                        yield return command;
                    }
                    else if (item is ModuleInfo subModule)
                    {
                        foreach (var subItem in Flatten(subModule.Components))
                        {
                            yield return subItem;
                        }
                    }
                }
            }

            return Flatten(Commands);
        }

        /// <summary>
        ///     Groups the command modules in <see cref="Commands"/> info a single enumerable collection. 
        /// </summary>
        /// <returns>A lazily evaluated, flattened <see cref="IEnumerable{T}"/> that holds all <see cref="ModuleInfo"/>'s registered in this manager.</returns>
        public IEnumerable<ModuleInfo> GetModules()
        {
            static IEnumerable<ModuleInfo> Flatten(IEnumerable<ISearchable> components)
            {
                foreach (var item in components)
                {
                    if (item is ModuleInfo subModule)
                    {
                        foreach (var subItem in Flatten(subModule.Components))
                        {
                            yield return subItem;
                        }

                        yield return subModule;
                    }
                }
            }

            return Flatten(Commands);
        }

        /// <summary>
        ///     Runs a thread safe search operation over all commands for any matches of <paramref name="args"/>.
        /// </summary>
        /// <param name="args">A set of arguments intended to discover commands as a query.</param>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> that holds the results of the search query.</returns>
        public IEnumerable<SearchResult> Search(ArgumentEnumerator args)
        {
            if (args == null)
            {
                ThrowHelpers.ThrowInvalidArgument(args);
            }

            return Commands.SearchMany(args, 0, false);
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">An unparsed input that is expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsyncMode"/> is set to <see cref="AsyncMode.Async"/>.</returns>
        public Task Execute<T>(
            T consumer, string args, CommandOptions? options = null)
            where T : ConsumerBase
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                ThrowHelpers.ThrowInvalidArgument(args);
            }

            return Execute(consumer, StringParser.ParseKeyValueCollection(args), options);
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases as <see cref="string"/> values.
        /// </remarks>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsyncMode"/> is set to <see cref="AsyncMode.Async"/>.</returns>
        public Task Execute<T>(
            T consumer, IEnumerable<object> args, CommandOptions? options = null)
            where T : ConsumerBase
        {
            return Execute(consumer, new ArgumentEnumerator(args), options ?? new CommandOptions());
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsyncMode"/> is set to <see cref="AsyncMode.Async"/>.</returns>
        public Task Execute<T>(
            T consumer, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
            where T : ConsumerBase
        {
            options ??= new CommandOptions();

            return Execute(consumer, new ArgumentEnumerator(args, options.MatchComparer), options);
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsyncMode"/> is set to <see cref="AsyncMode.Async"/>.</returns>
        public Task Execute<T>(
            T consumer, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            var task = Enter(consumer, args, options);

            if (options.AsyncMode is AsyncMode.Async)
                return Task.CompletedTask;

            return task;
        }

        private async Task Enter<T>(
            T consumer, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            ICommandResult? result = null;

            var searches = Search(args);
            foreach (var search in searches)
            {
                if (search.Component is CommandInfo command)
                {
                    result = await Run(consumer, command, search.SearchHeight, args, options);

                    if (!result.Success)
                    {
                        continue;
                    }

                    break;
                }

                result ??= search;
                continue;
            }

            result ??= SearchResult.FromError();

            await _disposer.Dispose(consumer, result, options);
        }

        private async ValueTask<ICommandResult> Run<T>(
            T consumer, CommandInfo command, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var conversion = await command.Convert(consumer, argHeight, args, options);

            var arguments = new object[conversion.Length];

            for (int i = 0; i < conversion.Length; i++)
            {
                if (!conversion[i].Success)
                    return MatchResult.FromError(command, conversion[i].Exception!);

                arguments[i] = conversion[i].Value!;
            }

            try
            {
                var preCheckResult = await command.EvaluatePreconditions(consumer, options);

                if (!preCheckResult.Success)
                {
                    return preCheckResult;
                }

                var value = command.Invoker.Invoke(consumer, command, arguments, this, options);

                var result = await command.HandleReturnType(consumer, value);

                var postCheckResult = await command.EvaluatePostconditions(consumer, options);

                if (!postCheckResult.Success)
                {
                    return postCheckResult;
                }

                return result;
            }
            catch (Exception exception)
            {
                return InvokeResult.FromError(command, exception);
            }
        }

        /// <summary>
        ///     Creates a builder that is responsible for setting up all required variables to create, search and run commands from a <see cref="CommandManager"/>.
        /// </summary>
        /// <remarks>
        ///     This builder is able to configure the following:
        ///     <list type="number">
        ///         <item>Defining assemblies through which will be searched to discover modules and commands.</item>
        ///         <item>Defining custom commands that do not appear in the assemblies.</item>
        ///         <item>Registering implementations of <see cref="TypeConverterBase"/> which define custom argument conversion.</item>
        ///         <item>Registering implementations of <see cref="ResultResolverBase"/> which define custom result handling.</item>
        ///         <item>Custom naming patterns that validate naming across the whole process.</item>
        ///     </list>
        /// </remarks>
        /// <returns>A new <see cref="ConfigurationBuilder"/> that implements the currently accessed <see cref="CommandManager"/>.</returns>
        public static ConfigurationBuilder CreateDefaultBuilder()
        {
            return new ConfigurationBuilder();
        }

        internal sealed class SequenceDisposer(IEnumerable<ResultResolverBase> eventHandlers)
        {
            private readonly ResultResolverBase[] _resolvers = eventHandlers.ToArray();

            internal async ValueTask Dispose(
                ConsumerBase consumer, ICommandResult result, CommandOptions options)
            {
                foreach (var resolver in _resolvers)
                {
                    await resolver.Evaluate(consumer, result, options.Services, options.CancellationToken);
                }
            }
        }
    }
}
