using Commands.Builders;
using Commands.Conditions;
using Commands.Resolvers;
using System.Diagnostics;
using System.Reflection;

[assembly: CLSCompliant(true)]

namespace Commands
{
    /// <summary>
    ///     The root type serving as a basis for all operations and functionality as provided by Commands.NET. This class cannot be inherited.
    ///     To learn more about use of this type and other features of Commands.NET, check out the README on GitHub: <see href="https://github.com/csmir/Commands.NET"/>
    /// </summary>
    /// <remarks>
    ///     To start using this tree, call <see cref="CreateBuilder"/> and configure it using the minimal API's implemented by the <see cref="ComponentTreeBuilder"/>.
    /// </remarks>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ComponentTree : ComponentCollection
    {
        private readonly ResultResolver[] _resolvers;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentTree"/> class, using the provided <paramref name="configuration"/> to build the command tree.
        /// </summary>
        /// <remarks>
        ///     This constructor searches for all components that inherit <see cref="CommandModule"/> in the provided assemblies.
        /// </remarks>
        /// <param name="configuration">The configuration by which all reflected components should be configured.</param>
        /// <param name="assemblies">A collection of assemblies through which a lookup will be executed to construct all components that inherit <see cref="CommandModule"/>.</param>
        public ComponentTree(ComponentConfiguration configuration, params IEnumerable<Assembly> assemblies)
            : this(configuration, assemblies, null, null) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentTree"/> class, using the provided <paramref name="configuration"/> to build the command tree.
        /// </summary>
        /// <remarks>
        ///     This constructor implements a collection of <see cref="IComponent"/> components that are passed to the tree.
        /// </remarks>
        /// <param name="configuration"></param>
        /// <param name="components"></param>
        public ComponentTree(ComponentConfiguration configuration, params IEnumerable<IComponent> components)
            : this(configuration, null, components, null) { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentTree"/> class, using the provided <paramref name="configuration"/> to build the command tree.
        /// </summary>
        /// <param name="configuration">The configuration by which all reflected components should be configured.</param>
        /// <param name="assemblies">An optional collection of assemblies through which a lookup will be executed to construct all components that inherit <see cref="CommandModule"/>.</param>
        /// <param name="runtimeComponents">Delegate-based components that should be passed to the tree at runtime.</param>
        /// <param name="resolvers">An optional collection of handlers of command results.</param>
        public ComponentTree(ComponentConfiguration configuration, IEnumerable<Assembly>? assemblies = null, IEnumerable<IComponent>? runtimeComponents = null, IEnumerable<ResultResolver>? resolvers = null)
            : base(false, null)
        {
            if (configuration.Properties is not Dictionary<string, object> properties)
                throw new ArgumentException($"The configuration properties must be an implementation of {nameof(Dictionary<string, object>)}.", nameof(configuration));

            assemblies ??= [];
            runtimeComponents ??= [];

            if (assemblies.Any() || runtimeComponents.Any())
            {
                properties["HierarchyRetentionHandler"] = new Action<IComponent[], bool>(HierarchyRetentionHandler);

                var commands = ComponentUtilities.GetTopLevelComponents(assemblies.ToArray(), configuration)
                    .Concat(runtimeComponents)
                    .OrderByDescending(command => command.Score);

                PushDangerous(commands);
            }

            _resolvers = resolvers?.ToArray() ?? [];
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

                if (component is ModuleInfo module)
                    discovered.AddRange(module.Find(args));
                else
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
            }

            return discovered;
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">An unparsed input that is expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public Task Execute<T>(
            T caller, string args, CommandOptions? options = null)
            where T : CallerContext
        {
            if (string.IsNullOrWhiteSpace(args))
                throw new ArgumentNullException(nameof(args));

            return Execute(caller, ArgumentParser.ParseKeyValueCollection(args), options);
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases as <see cref="string"/> values.
        /// </remarks>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public Task Execute<T>(
            T caller, IEnumerable<object> args, CommandOptions? options = null)
            where T : CallerContext
        {
            return Execute(caller, new ArgumentEnumerator(args), options ?? new CommandOptions());
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public Task Execute<T>(
            T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
            where T : CallerContext
        {
            options ??= new CommandOptions();

            return Execute(caller, new ArgumentEnumerator(args, options.MatchComparer), options);
        }

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public Task Execute<T>(
            T caller, ArgumentEnumerator args, CommandOptions options)
            where T : CallerContext
        {
            var task = StartAsynchronousPipeline(caller, args, options);

            if (options.DoAsynchronousExecution)
                return Task.CompletedTask;

            return task;
        }

        #region Pipeline

        private async Task StartAsynchronousPipeline<T>(
            T caller, ArgumentEnumerator args, CommandOptions options)
            where T : CallerContext
        {
            IExecuteResult? result = null;

            var searches = Find(args);
            foreach (var search in searches)
            {
                if (search.Component is CommandInfo command)
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

            await FinalizeInvocation(caller, result, options);
        }

        private async ValueTask<IExecuteResult> InvokeCommand<T>(
            T caller, CommandInfo command, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : CallerContext
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var beforeConvertConditions = await EvaluateConditions(caller, command, ConditionTrigger.BeforeConversion, options);

            if (!beforeConvertConditions.Success)
                return beforeConvertConditions;

            var conversion = await ConvertCommand(caller, command, argHeight, args, options);

            var arguments = new object[conversion.Length];

            for (int i = 0; i < conversion.Length; i++)
            {
                if (!conversion[i].Success)
                    return MatchResult.FromError(command, conversion[i].Exception!);

                arguments[i] = conversion[i].Value!;
            }

            try
            {
                var beforeInvocationConditions = await EvaluateConditions(caller, command, ConditionTrigger.BeforeInvocation, options);

                if (!beforeInvocationConditions.Success)
                    return beforeInvocationConditions;

                var value = command.Activator.Invoke(caller, command, arguments, this, options);

                await EvaluateInvocationResult(caller, command, value, options);

                var afterInvocationConditions = await EvaluateConditions(caller, command, ConditionTrigger.AfterInvocation, options);

                if (!afterInvocationConditions.Success)
                    return afterInvocationConditions;

                return InvokeResult.FromSuccess(command);
            }
            catch (Exception exception)
            {
                return InvokeResult.FromError(command, exception);
            }
        }

        private async ValueTask<ConvertResult[]> ConvertCommand<T>(
            T caller, CommandInfo command, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : CallerContext
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            args.SetSize(argHeight);

            if (!command.HasArguments && args.Length == 0)
                return [];

            if (command.MaxLength == args.Length)
                return await ConvertArguments(caller, command.Arguments, args, options);

            if (command.MaxLength <= args.Length && command.HasRemainder)
                return await ConvertArguments(caller, command.Arguments, args, options);

            if (command.MaxLength > args.Length && command.MinLength <= args.Length)
                return await ConvertArguments(caller, command.Arguments, args, options);

            return [ConvertResult.FromError()];
        }

        private async ValueTask<ConvertResult[]> ConvertArguments(
            CallerContext caller, IArgument[] arguments, ArgumentEnumerator args, CommandOptions options)
        {
            static ValueTask<ConvertResult> Convert(IArgument argument, CallerContext caller, object? value, bool isArray, CommandOptions options)
            {
                if (argument.IsNullable && value is null or "null")
                    return ConvertResult.FromSuccess(null);

                if (value is null)
                    return ConvertResult.FromError(new ArgumentNullException(argument.Name));

                if (argument.Type.IsString())
                    return ConvertResult.FromSuccess(value?.ToString());

                if (argument.Type.IsObject())
                    return ConvertResult.FromSuccess(value);

                return argument.Converter!.Evaluate(caller, argument, value, options.Services, options.CancellationToken);
            }

            options.CancellationToken.ThrowIfCancellationRequested();

            var results = new ConvertResult[arguments.Length];

            // loop all arguments.
            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];

                // parse remainder.
                if (argument.IsRemainder)
                {
                    results[i] = await Convert(argument, caller, argument.IsCollection ? args.TakeRemaining() : args.JoinRemaining(options.RemainderSeparator), true, options);

                    // End of the line, as remainder is always the last argument.
                    break;
                }

                // parse complex argument.
                if (argument is ComplexArgumentInfo complexArgument)
                {
                    var result = await ConvertArguments(caller, complexArgument.Arguments, args, options);

                    if (result.All(x => x.Success))
                    {
                        try
                        {
                            var instance = complexArgument.Activator.Invoke(caller, null, result.Select(x => x.Value).ToArray(), null, options);
                            results[i] = ConvertResult.FromSuccess(instance);
                        }
                        catch (Exception ex)
                        {
                            results[i] = ConvertResult.FromError(ex);
                        }

                        continue;
                    }

                    if (complexArgument.IsOptional)
                        results[i] = ConvertResult.FromSuccess(Type.Missing);

                    // continue looking for more args
                    continue;
                }

                if (args.TryNext(argument.Name!, out var value))
                {
                    results[i] = await Convert(argument, caller, value, false, options);
                    continue;
                }

                if (argument.IsOptional)
                {
                    results[i] = ConvertResult.FromSuccess(Type.Missing);
                    continue;
                }

                results[i] = ConvertResult.FromError(new ArgumentNullException(argument.Name));
            }

            return results;
        }

        private async ValueTask<ConditionResult> EvaluateConditions<T>(
            T caller, CommandInfo command, ConditionTrigger trigger, CommandOptions options)
            where T : CallerContext
        {
            if (!options.SkipConditions)
            {
                foreach (var condition in command.Conditions)
                {
                    if (condition.Trigger.HasFlag(trigger))
                    {
                        var checkResult = await condition.Evaluate(caller, command, trigger, options.Services, options.CancellationToken);

                        if (!checkResult.Success)
                            return checkResult;
                    }
                }
            }

            return ConditionResult.FromSuccess(trigger);
        }

        private async ValueTask EvaluateInvocationResult(
            CallerContext caller, CommandInfo command, object? value, CommandOptions options)
        {
            foreach (var resolver in _resolvers)
                await resolver.EvaluateResponse(caller, command, value, options.Services, options.CancellationToken);
        }

        private async ValueTask FinalizeInvocation(
            CallerContext caller, IExecuteResult result, CommandOptions options)
        {
            foreach (var resolver in _resolvers)
                await resolver.EvaluateResult(caller, result, options.Services, options.CancellationToken);
        }

        #endregion

        // This method is called when a top-level commands' module receives a mutation which prompts a re-sort of the hierarchy.
        private void HierarchyRetentionHandler(IComponent[] newComponents, bool removing = false)
        {
            if (removing)
                RemoveRange(newComponents);
            else
            {
                AddRange(newComponents);

                // Only re-sort if the new components are being added.
                // It is unnecessary to re-sort if they are being removed, as the hierarchy will be re-sorted on the next addition.
                Sort();
            }
        }

        /// <summary>
        ///     Creates a builder that is responsible for setting up all required variables to create, search and run commands from a <see cref="ComponentTree"/>. This builder is pre-configured with default settings.
        /// </summary>
        /// <returns>A new instance of <see cref="ComponentTreeBuilder"/> that builds into a new instance of the <see cref="ComponentTree"/>.</returns>
        public static ITreeBuilder CreateBuilder()
            => new ComponentTreeBuilder();
    }
}
