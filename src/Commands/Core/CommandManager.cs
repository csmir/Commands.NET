using Commands.Exceptions;
using Commands.Helpers;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.TypeConverters;
using System.Collections.Concurrent;
using System.Diagnostics;

[assembly: CLSCompliant(true)]

namespace Commands
{
    /// <summary>
    ///     The root type serving as a basis for all operations and functionality as provided by Commands.NET. 
    ///     To learn more about use of this type and other features of Commands.NET, check out the README on GitHub: <see href="https://github.com/csmir/Commands.NET"/>
    /// </summary>
    /// <remarks>
    ///     This type is not sealed, and can be implemented to override the virtual execution steps. A hosted implementation exists in the <b>Commands.NET.Hosting</b> package,
    ///     which introduces native IoC based command operations alongside the preexisting DI support.
    ///     <br/>
    ///     <br/>
    ///     To start using the manager, configure it using the <see cref="CommandBuilder{T}"/> and calling <see cref="CommandBuilder{T}.Build(object[])"/>. 
    ///     <see cref="CommandManager.CreateBuilder"/> generates a base definition of this builder to use.
    /// </remarks>
    [DebuggerDisplay("Commands = {Commands},nq")]
    public class CommandManager : IDisposable
    {
        private readonly CommandFinalizer _finalizer;
        private readonly ConcurrentDictionary<Guid, Task> _taskTrace;

        /// <summary>
        ///     Gets the collection containing all commands, named modules and subcommands as implemented by the assemblies that were registered in the <see cref="ICommandBuilder"/> provided when creating the manager.
        /// </summary>
        public HashSet<ISearchable> Commands { get; }

        /// <summary>
        ///     Creates a new <see cref="CommandManager"/> based on the provided arguments.
        /// </summary>
        /// <param name="builder">The options through which to construct the collection of <see cref="Commands"/>.</param>
        public CommandManager(ICommandBuilder builder)
        {
            _finalizer = new CommandFinalizer(builder.ResultResolvers);
            _taskTrace = [];

            var commands = ReflectionUtilities.GetTopLevelComponents(builder)
                .Concat(builder.Commands)
                .OrderByDescending(command => command.Score);

            Commands = [.. commands];
        }

        /// <summary>
        ///     Flattens the top level commands in <see cref="Commands"/> into a single enumerable collection.
        /// </summary>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> that holds the flattened range of <see cref="CommandInfo"/>'s registered in this manager.</returns>
        public virtual IEnumerable<CommandInfo> GetCommands()
        {
            static IEnumerable<CommandInfo> Step(ModuleInfo module)
            {
                foreach (var item in module.Components)
                {
                    if (item is CommandInfo command)
                    {
                        yield return command;
                    }
                    else if (item is ModuleInfo subModule)
                    {
                        foreach (var subItem in Step(subModule))
                        {
                            yield return subItem;
                        }
                    }
                }
            }

            foreach (var command in Commands)
            {
                if (command is CommandInfo c)
                {
                    yield return c;
                }
                else if (command is ModuleInfo m)
                {
                    foreach (var subItem in Step(m))
                    {
                        yield return subItem;
                    }
                }
            }
        }

        /// <summary>
        ///     Groups the command modules in <see cref="Commands"/> info a single enumerable collection. 
        /// </summary>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> that holds the range of <see cref="ModuleInfo"/>'s registered in this manager.</returns>
        public virtual IEnumerable<ModuleInfo> GetModules()
        {
            static IEnumerable<ModuleInfo> Step(ModuleInfo module)
            {
                foreach (var item in module.Components)
                {
                    if (item is ModuleInfo subModule)
                    {
                        foreach (var subItem in Step(subModule))
                        {
                            yield return subItem;
                        }


                        yield return subModule;
                    }
                }
            }

            foreach (var command in Commands)
            {
                if (command is ModuleInfo m)
                {
                    foreach (var subItem in Step(m))
                    {
                        yield return subItem;
                    }

                    yield return m;
                }
            }
        }

        /// <summary>
        ///     Makes an attempt at executing a command from provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases this way also.
        /// </remarks>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsyncMode"/> is set to <see cref="AsyncMode.Async"/>.</returns>
        public virtual Task TryExecuteAsync<T>(
            T consumer, object[] args, CommandOptions? options = default)
            where T : ConsumerBase
        {
            var traceId = Guid.NewGuid();

            ArgumentNullException.ThrowIfNull(args, nameof(args));
            ArgumentNullException.ThrowIfNull(consumer, nameof(consumer));

            options ??= new CommandOptions();

            var execution = ExecuteAsync(consumer, args, options);

            if (options.AsyncMode is AsyncMode.Await)
                return execution;

            _taskTrace.TryAdd(traceId, execution);

            execution.ContinueWith(x =>
            {
                if (_taskTrace.ContainsKey(traceId))
                    _taskTrace.TryRemove(traceId, out _);
            });

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Runs a thread safe search operation over all commands for any matches of <paramref name="args"/>.
        /// </summary>
        /// <param name="args">A set of arguments intended to discover commands as a query.</param>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> that holds the results of the search query.</returns>
        public virtual IEnumerable<SearchResult> Search(object[] args)
        {
            if (args == null)
            {
                ThrowHelpers.ThrowInvalidArgument(args);
            }

            return Commands.SearchMany(args, 0, false);
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
            options.CancellationToken.ThrowIfCancellationRequested();

            ICommandResult? result = null;

            var searches = Search(args);

            foreach (var search in searches)
            {
                if (search.Component is CommandInfo command)
                {
                    result = await RunAsync(consumer, command, search.SearchHeight, args, options);

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

            await _finalizer.FinalizeAsync(consumer, result, options);
        }

        /// <summary>
        ///     Invokes the provided <paramref name="command"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="command">The result of the match intended to be ran.</param>
        /// <param name="argHeight">The height at which the command name ends and argument input starts.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the invocation process.</returns>
        protected virtual async ValueTask<ICommandResult> RunAsync<T>(
            T consumer, CommandInfo command, int argHeight, object[] args, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var conversion = await ConvertAsync(consumer, command, argHeight, args, options);

            var arguments = new object[conversion.Length];

            for (int i = 0; i < conversion.Length; i++)
            {
                if (!conversion[i].Success)
                    return MatchResult.FromError(command, conversion[i].Exception!);

                arguments[i] = conversion[i].Value!;
            }

            try
            {
                var preCheckResult = await CheckPreconditionsAsync(consumer, command, options);

                if (!preCheckResult.Success)
                {
                    return preCheckResult;
                }

                var value = command.Invoker.Invoke(consumer, command, arguments, this, options);

                var result = await HandleReturnTypeAsync(consumer, command, value, options);

                var postCheckResult = await CheckPostconditionsAsync(consumer, command, options);

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
        ///     Handles the returned result from a command invocation by, if necessary, sending the result to the consumer.
        /// </summary>
        /// <param name="consumer">A command context that persist for the duration of the execution pipeline, serving as a metadata and logging container.</param>
        /// <param name="command">The result of the match intended to be ran.</param>
        /// <param name="value">The returned result of this operation.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the handling process.</returns>
        protected virtual async ValueTask<InvokeResult> HandleReturnTypeAsync<T>(
            T consumer, CommandInfo command, object? value, CommandOptions options)
            where T : ConsumerBase
        {
            switch (value)
            {
                case null: // (void)
                    {
                        return InvokeResult.FromSuccess(command);
                    }
                case Task awaitablet:
                    {
                        await awaitablet;

                        var vtType = awaitablet.GetType();

                        if (vtType.IsGenericType)
                        {
                            var result = vtType.GetProperty("Result")?.GetValue(awaitablet);

                            if (result != null)
                            {
                                await consumer.SendAsync(result.ToString() ?? string.Empty);
                            }
                        }

                        return InvokeResult.FromSuccess(command);
                    }
                case ValueTask awaitablevt:
                    {
                        await awaitablevt;

                        var vtType = awaitablevt.GetType();

                        if (vtType.IsGenericType)
                        {
                            var result = vtType.GetProperty("Result")?.GetValue(awaitablevt);

                            if (result != null)
                            {
                                await consumer.SendAsync(result.ToString() ?? string.Empty);
                            }
                        }

                        return InvokeResult.FromSuccess(command);
                    }
                case object obj:
                    {
                        if (obj != null)
                        {
                            await consumer.SendAsync(obj.ToString() ?? string.Empty);
                        }

                        return InvokeResult.FromSuccess(command);
                    }
            }
        }

        private async ValueTask<ConvertResult[]> ConvertAsync<T>(
            T consumer, CommandInfo command, int argHeight, object[] args, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var length = args.Length - argHeight;

            if (command.HasArguments)
            {
                if (command.MaxLength == length)
                {
                    return await command.Arguments.ConvertManyAsync(consumer, args[^length..], 0, options);
                }

                if (command.MaxLength <= length && command.HasRemainder)
                {
                    return await command.Arguments.ConvertManyAsync(consumer, args[^length..], 0, options);
                }

                if (command.MaxLength > length && command.MinLength <= length)
                {
                    return await command.Arguments.ConvertManyAsync(consumer, args[^length..], 0, options);
                }
            }
            else if (length == 0)
            {
                return [];
            }

            return [ConvertResult.FromError(ConvertException.ArgumentMismatch())];
        }

        private async ValueTask<ConditionResult> CheckPreconditionsAsync<T>(
            T consumer, CommandInfo command, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            if (!options.SkipPreconditions)
            {
                foreach (var precon in command.Preconditions)
                {
                    var checkResult = await precon.EvaluateAsync(consumer, command, options.Services, options.CancellationToken);

                    if (!checkResult.Success)
                    {
                        return checkResult;
                    }
                }
            }

            return ConditionResult.FromSuccess();
        }

        private async ValueTask<ConditionResult> CheckPostconditionsAsync<T>(
            T consumer, CommandInfo command, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            if (!options.SkipPostconditions)
            {
                foreach (var postcon in command.PostConditions)
                {
                    var checkResult = await postcon.EvaluateAsync(consumer, command, options.Services, options.CancellationToken);

                    if (!checkResult.Success)
                    {
                        return checkResult;
                    }
                }
            }

            return ConditionResult.FromSuccess();
        }

        /// <summary>
        ///     Disposes this manager.
        /// </summary>
        /// <remarks>
        ///     This dispose method will wait for all running command tasks to complete before disposing of the manager.
        /// </remarks>
        public void Dispose()
        {
            Task.WaitAll([.. _taskTrace.Values]);
        }

        /// <summary>
        ///     Creates a builder that is responsible for setting up all required arguments to discover and populate <see cref="Commands"/>.
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
        /// <returns>A new <see cref="ICommandBuilder"/> that implements <see cref="CommandManager"/></returns>
        public static CommandBuilder<CommandManager> CreateBuilder()
        {
            return new CommandBuilder<CommandManager>();
        }
    }
}
