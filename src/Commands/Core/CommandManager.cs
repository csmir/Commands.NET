using Commands.Converters;
using Commands.Parsing;
using Commands.Reflection;
using Commands.Resolvers;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

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
    [DebuggerDisplay("Components = {Count},nq")]
    public sealed class CommandManager : ISearchableSet
    {
        private HashSet<ISearchable> _components;
        private readonly ResultEnumerator _resultEnumerator;

        /// <inheritdoc />
        public int Count
            => _components.Count;

        /// <inheritdoc />
        public bool IsReadOnly
            => false;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommandManager"/> class.
        /// </summary>
        /// <param name="configuration">The configuration by which all reflected components should be configured.</param>
        /// <param name="assemblies">An optional collection of assemblies through which a lookup will be executed to construct all components that inherit <see cref="ModuleBase"/> within the provided assemblies.</param>
        /// <param name="resolvers">An optional collection of handlers of command results.</param>
        /// <param name="runtimeComponents">Delegate-based components that should be passed to the manager at runtime.</param>
        public CommandManager(BuildConfiguration configuration, IEnumerable<Assembly>? assemblies = null, IEnumerable<ResultResolverBase>? resolvers = null, IEnumerable<ISearchable>? runtimeComponents = null)
        {
            resolvers ??= [];
            assemblies ??= [];
            runtimeComponents ??= [];

            _resultEnumerator = new ResultEnumerator(resolvers);

            if (assemblies.Any() || runtimeComponents.Any())
            {
                configuration.N_NotifyTopLevelMutation = NotifyTreeMutation;

                var commands = ReflectionUtilities.GetTopLevelComponents(assemblies.ToArray(), configuration)
                    .Concat(runtimeComponents)
                    .OrderByDescending(command => command.Score);

                _components = [.. commands];
            }
            else
                _components = [];
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
            => new();

        /// <inheritdoc />
        public IEnumerable<SearchResult> Find(ArgumentEnumerator args)
        {
            List<SearchResult> discovered = [];

            var searchHeight = 0;

            foreach (var component in _components)
            {
                if (args.Length == searchHeight)
                    continue;

                if (!args.TryNext(searchHeight, out var value))
                    continue;

                if (!component.Aliases.Any(x => x == value))
                    continue;

                if (component is ModuleInfo module)
                {
                    var nested = module.Find(args);

                    discovered.AddRange(nested);

                    discovered.Add(SearchResult.FromError(module));
                }
                else
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
            }

            return discovered;
        }

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetCommands(bool browseNestedComponents = true)
            => GetCommands(_ => true, browseNestedComponents);

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetCommands(Predicate<CommandInfo> predicate, bool browseNestedComponents = true)
        {
            if (!browseNestedComponents)
                return _components.Where(x => x is CommandInfo info && predicate(info));

            List<ISearchable> discovered = [];

            foreach (var component in _components)
            {
                if (component is CommandInfo command && predicate(command))
                    discovered.Add(command);

                if (component is ModuleInfo module)
                    discovered.AddRange(module.GetCommands(predicate, browseNestedComponents));
            }

            return discovered;
        }

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetModules(bool browseNestedComponents = true)
            => GetModules(_ => true, browseNestedComponents);

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetModules(Predicate<ModuleInfo> predicate, bool browseNestedComponents = true)
        {
            if (!browseNestedComponents)
                return _components.Where(x => x is ModuleInfo info && predicate(info));

            List<ISearchable> discovered = [];

            foreach (var component in _components)
            {
                if (component is ModuleInfo module)
                {
                    if (predicate(module))
                        discovered.Add(component);

                    discovered.AddRange(module.GetModules(predicate, browseNestedComponents));
                }
            }

            return discovered;
        }

        /// <inheritdoc />
        public IEnumerable<ISearchable> GetAll()
            => _components;

        /// <inheritdoc />
        public int CountAll()
        {
            var sum = 0;
            foreach (var component in _components)
            {
                if (component is ModuleInfo module)
                    sum += module.CountAll();

                sum++;
            }

            return sum;
        }

        /// <inheritdoc />
        public bool Add(ISearchable component)
            => AddRange(component) > 0;

        /// <inheritdoc />
        public int AddRange(params ISearchable[] components)
        {
            var hasChanged = 0;

            var copy = new HashSet<ISearchable>(_components);

            foreach (var component in components)
                hasChanged += (copy.Add(component) ? 1 : 0);

            if (hasChanged > 0)
            {
                var orderedCopy = new HashSet<ISearchable>(copy.OrderByDescending(x => x.Score));

                Interlocked.Exchange(ref _components, orderedCopy);
            }

            return hasChanged;
        }

        /// <inheritdoc />
        public bool Remove(ISearchable component)
            => _components.Remove(component);

        /// <inheritdoc />
        public int RemoveWhere(Predicate<ISearchable> predicate)
            => _components.RemoveWhere(predicate);

        /// <inheritdoc />
        public IEnumerator<ISearchable> GetEnumerator()
            => _components.GetEnumerator();

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="consumer">A command consumer that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">An unparsed input that is expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public Task Execute<T>(
            T consumer, string args, CommandOptions? options = null)
            where T : ConsumerBase
        {
            if (string.IsNullOrWhiteSpace(args))
                throw new ArgumentNullException(nameof(args));

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
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
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
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
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
        /// <returns>An awaitable <see cref="Task"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public Task Execute<T>(
            T consumer, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            var task = StartAsynchronousPipeline(consumer, args, options);

            if (options.DoAsynchronousExecution)
                return Task.CompletedTask;

            return task;
        }

        private async Task StartAsynchronousPipeline<T>(
            T consumer, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            ICommandResult? result = null;

            var searches = Find(args);
            foreach (var search in searches)
            {
                if (search.Component is CommandInfo command)
                {
                    result = await InvokeCommand(consumer, command, search.SearchHeight, args, options);

                    if (!result.Success)
                        continue;

                    break;
                }

                result ??= search;
                continue;
            }

            result ??= SearchResult.FromError();

            await _resultEnumerator.Dispose(consumer, result, options);
        }

        private async ValueTask<ICommandResult> InvokeCommand<T>(
            T consumer, CommandInfo command, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var conversion = await ConvertCommand(consumer, command, argHeight, args, options);

            var arguments = new object[conversion.Length];

            for (int i = 0; i < conversion.Length; i++)
            {
                if (!conversion[i].Success)
                    return MatchResult.FromError(command, conversion[i].Exception!);

                arguments[i] = conversion[i].Value!;
            }

            try
            {
                var preCheckResult = await EvaluatePreconditions(consumer, command, options);

                if (!preCheckResult.Success)
                    return preCheckResult;

                var value = command.Invoker.Invoke(consumer, command, arguments, this, options);

                await _resultEnumerator.Respond(consumer, command, value, options);

                var postCheckResult = await EvaluatePostconditions(consumer, command, options);

                if (!postCheckResult.Success)
                    return postCheckResult;

                return InvokeResult.FromSuccess(command);
            }
            catch (Exception exception)
            {
                return InvokeResult.FromError(command, exception);
            }
        }

        private async ValueTask<ConvertResult[]> ConvertCommand<T>(
            T consumer, CommandInfo command, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            args.SetSize(argHeight);

            if (!command.HasArguments && args.Length == 0)
                return [];

            if (command.MaxLength == args.Length)
                return await ConvertArguments(consumer, command.Arguments, args, options);

            if (command.MaxLength <= args.Length && command.HasRemainder)
                return await ConvertArguments(consumer, command.Arguments, args, options);

            if (command.MaxLength > args.Length && command.MinLength <= args.Length)
                return await ConvertArguments(consumer, command.Arguments, args, options);

            return [ConvertResult.FromError(ConvertException.ArgumentMismatch())];
        }

        private async ValueTask<ConvertResult[]> ConvertArguments(
            ConsumerBase consumer, IArgument[] arguments, ArgumentEnumerator args, CommandOptions options)
        {
            static ValueTask<ConvertResult> Convert(IArgument argument, ConsumerBase consumer, object? value, bool isArray, CommandOptions options)
            {
                options.CancellationToken.ThrowIfCancellationRequested();

                if (argument.IsNullable && value is null or "null")
                    return ConvertResult.FromSuccess();

                if (value is null)
                    return ConvertResult.FromError(new ArgumentNullException(argument.Name));

                if (argument.Type.IsString())
                    return ConvertResult.FromSuccess(value?.ToString());

                if (argument.Type.IsObject())
                    return ConvertResult.FromSuccess(value);

                return argument.Converter!.Evaluate(consumer, argument, value, options.Services, options.CancellationToken);
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
                    results[i] = await Convert(argument, consumer, argument.IsCollection ? args.TakeRemaining() : args.JoinRemaining(), true, options);

                    // End of the line, as remainder is always the last argument.
                    break;
                }

                // parse complex argument.
                if (argument is ComplexArgumentInfo complexArgument)
                {
                    var result = await ConvertArguments(consumer, complexArgument.Arguments, args, options);

                    if (result.All(x => x.Success))
                    {
                        try
                        {
                            var instance = complexArgument.Constructor.Invoke(result.Select(x => x.Value).ToArray());
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
                    results[i] = await Convert(argument, consumer, value, false, options);
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

        private async ValueTask<ConditionResult> EvaluatePreconditions<T>(
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
                        return checkResult;
                }
            }

            return ConditionResult.FromSuccess();
        }

        private async ValueTask<ConditionResult> EvaluatePostconditions<T>(
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
                        return checkResult;
                }
            }

            return ConditionResult.FromSuccess();
        }

        private void NotifyTreeMutation(params ISearchable[] newComponents)
        {
            var copy = new HashSet<ISearchable>(_components);

            foreach (var component in newComponents)
                copy.Add(component);

            var orderedCopy = new HashSet<ISearchable>(_components.OrderByDescending(x => x.Score));

            Interlocked.Exchange(ref _components, orderedCopy);
        }

        internal readonly struct ResultEnumerator(IEnumerable<ResultResolverBase> eventHandlers)
        {
            private readonly ResultResolverBase[] _resolvers = eventHandlers.ToArray();

            internal async ValueTask Dispose(
                ConsumerBase consumer, ICommandResult result, CommandOptions options)
            {
                foreach (var resolver in _resolvers)
                    await resolver.Evaluate(consumer, result, options.Services, options.CancellationToken);
            }

            internal async ValueTask Respond(
                ConsumerBase consumer, CommandInfo command, object? value, CommandOptions options)
            {
                foreach (var resolver in _resolvers)
                    await resolver.Respond(consumer, command, value, options.Services, options.CancellationToken);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
