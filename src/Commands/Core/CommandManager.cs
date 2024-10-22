using Commands.Exceptions;
using Commands.Helpers;
using Commands.Reflection;
using Commands.Resolvers;
using Commands.TypeConverters;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
    public class CommandManager
    {
        private readonly CommandFinalizer _finalizer;

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
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> that holds the range of <see cref="ModuleInfo"/>'s registered in this manager.</returns>
        public virtual IEnumerable<ModuleInfo> GetModules()
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
        public virtual IEnumerable<SearchResult> Search(object[] args)
        {
            if (args == null)
            {
                ThrowHelpers.ThrowInvalidArgument(args);
            }

            return Commands.SearchMany(args, 0, false);
        }

        /// <summary>
        ///     Makes an attempt at executing a command from the provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases this way also.
        /// </remarks>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.AsyncMode"/> is set to <see cref="AsyncMode.Async"/>.</returns>
        public virtual Task Execute<T>(
            [DisallowNull] T consumer, object[] args, CommandOptions? options = default)
            where T : ConsumerBase
        {
            options ??= new CommandOptions();

            var task = Enter(consumer, args, options);

            if (options.AsyncMode is AsyncMode.Async)
                return Task.CompletedTask;

            return task;
        }

        /// <summary>
        ///     Steps through the pipeline in order to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution.</returns>
        protected virtual async Task Enter<T>(
            T consumer, object[] args, CommandOptions options)
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

            await _finalizer.Finalize(consumer, result, options);
        }

        /// <summary>
        ///     Invokes the provided <paramref name="command"/> and returns the result.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="command">The result of the match intended to be ran.</param>
        /// <param name="argHeight">The height at which the command name ends and argument input starts.</param>
        /// <param name="args">A set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the invocation process.</returns>
        protected virtual async ValueTask<ICommandResult> Run<T>(
            T consumer, CommandInfo command, int argHeight, object[] args, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var conversion = await Convert(consumer, command, argHeight, args, options);

            var arguments = new object[conversion.Length];

            for (int i = 0; i < conversion.Length; i++)
            {
                if (!conversion[i].Success)
                    return MatchResult.FromError(command, conversion[i].Exception!);

                arguments[i] = conversion[i].Value!;
            }

            try
            {
                var preCheckResult = await CheckPreconditions(consumer, command, options);

                if (!preCheckResult.Success)
                {
                    return preCheckResult;
                }

                var value = command.Invoker.Invoke(consumer, command, arguments, this, options);

                var result = await HandleReturnType(consumer, command, value, options);

                var postCheckResult = await CheckPostconditions(consumer, command, options);

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
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="command">The result of the match intended to be ran.</param>
        /// <param name="value">The returned result of this operation.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the handling process.</returns>
        protected virtual async ValueTask<InvokeResult> HandleReturnType<T>(
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

                        var type = command.Invoker.GetReturnType()!;

                        if (type.IsGenericType)
                        {
                            var result = type.GetProperty("Result")?.GetValue(awaitablet);

                            if (result != null)
                            {
                                await consumer.Send(result);
                            }
                        }

                        return InvokeResult.FromSuccess(command);
                    }
                case ValueTask awaitablevt:
                    {
                        await awaitablevt;

                        var type = command.Invoker.GetReturnType()!;

                        if (type.IsGenericType)
                        {
                            var result = type.GetProperty("Result")?.GetValue(awaitablevt);

                            if (result != null)
                            {
                                await consumer.Send(result);
                            }
                        }

                        return InvokeResult.FromSuccess(command);
                    }
                case object obj:
                    {
                        if (obj != null)
                        {
                            await consumer.Send(obj);
                        }

                        return InvokeResult.FromSuccess(command);
                    }
            }
        }

        /// <summary>
        ///     Converts the provided <paramref name="args"/> into a set of arguments that can be used to invoke a command.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="command">The result of the match intended to be ran.</param>
        /// <param name="argHeight">The argument level from search operation.</param>
        /// <param name="args">The arguments intended to populate the command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the conversion process.</returns>
        protected async ValueTask<ConvertResult[]> Convert<T>(
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

        /// <summary>
        ///     Checks the preconditions of a command before invoking it.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="command">The result of the match intended to be ran.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the evaluation process.</returns>
        protected async ValueTask<ConditionResult> CheckPreconditions<T>(
            T consumer, CommandInfo command, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            if (!options.SkipPreconditions)
            {
                foreach (var precon in command.PreEvaluations)
                {
                    var checkResult = await precon.Evaluate(consumer, command, options.Services, options.CancellationToken);

                    if (!checkResult.Success)
                    {
                        return checkResult;
                    }
                }
            }

            return ConditionResult.FromSuccess();
        }

        /// <summary>
        ///     Checks the postconditions of a command before invoking it.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="command">The result of the match intended to be ran.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> holding the result of the evaluation process.</returns>
        protected async ValueTask<ConditionResult> CheckPostconditions<T>(
            T consumer, CommandInfo command, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            if (!options.SkipPostconditions)
            {
                foreach (var postcon in command.PostEvaluations)
                {
                    var checkResult = await postcon.Evaluate(consumer, command, options.Services, options.CancellationToken);

                    if (!checkResult.Success)
                    {
                        return checkResult;
                    }
                }
            }

            return ConditionResult.FromSuccess();
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
