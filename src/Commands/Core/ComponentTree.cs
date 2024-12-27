using Commands.Builders;
using Commands.Conditions;
using System.Diagnostics;

namespace Commands
{
    /// <inheritdoc cref="IComponentTree"/>
    [DebuggerDisplay("Count = {Count}")]
    public sealed class ComponentTree : ComponentCollection, IComponentTree
    {
        private readonly ResultHandler[] _handlers;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ComponentTree"/> class, which serves as a container for executing commands and modules.
        /// </summary>
        /// <param name="components">A collection of executable components that can be executed by calling <see cref="Execute{T}(T, string, CommandOptions?)"/> or any overload of the same method.</param>
        /// <param name="handlers">A collection of <see cref="ResultHandler"/> implementations that handle results returned by the handler.</param>
        public ComponentTree(IEnumerable<IComponent> components, params ResultHandler[] handlers)
            : base(false)
        {
            var topLevelComponents = new List<IComponent>();

            foreach (var component in components)
            {
                if (component.IsSearchable)
                    topLevelComponents.Add(component);

                if (component is ComponentCollection collection)
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

                if (component is ModuleInfo module)
                    discovered.AddRange(module.Find(args));
                else
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
            }

            return discovered;
        }

        /// <inheritdoc />
        public Task Execute<T>(
            T caller, string args, CommandOptions? options = null)
            where T : ICallerContext
#if NET8_0_OR_GREATER
            => Execute(caller, ArgumentParser.ParseKeyValueCollection(args), options);
#else
            => Execute(caller, ArgumentParser.ParseKeyCollection(args), options);
#endif

        /// <inheritdoc />
        public Task Execute<T>(
            T caller, IEnumerable<object> args, CommandOptions? options = null)
            where T : ICallerContext
            => Execute(caller, new ArgumentEnumerator(args), options ?? new CommandOptions());

        /// <inheritdoc />
        public Task Execute<T>(
            T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
            where T : ICallerContext
        {
            options ??= new CommandOptions();

            return Execute(caller, new ArgumentEnumerator(args, options.MatchComparer), options);
        }

        /// <inheritdoc />
        public Task Execute<T>(
            T caller, ArgumentEnumerator args, CommandOptions options)
            where T : ICallerContext
        {
            var task = StartAsynchronousPipeline(caller, args, options);

            if (options.DoAsynchronousExecution)
                return Task.CompletedTask;

            return task;
        }

        #region Pipeline

        private async Task StartAsynchronousPipeline<T>(
            T caller, ArgumentEnumerator args, CommandOptions options)
            where T : ICallerContext
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

        private async Task<IExecuteResult> InvokeCommand<T>(
            T caller, CommandInfo command, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : ICallerContext
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var beforeConvertConditions = await CheckConditions(caller, command, ConditionTrigger.BeforeParse, options);

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
                var beforeInvocationConditions = await CheckConditions(caller, command, ConditionTrigger.BeforeInvoke, options);

                if (!beforeInvocationConditions.Success)
                    return beforeInvocationConditions;

                var value = command.Activator.Invoke(caller, command, arguments, this, options);

                var afterInvocationConditions = await CheckConditions(caller, command, ConditionTrigger.BeforeResult, options);

                if (!afterInvocationConditions.Success)
                    return afterInvocationConditions;

                return InvokeResult.FromSuccess(command, value);
            }
            catch (Exception exception)
            {
                return InvokeResult.FromError(command, exception);
            }
        }

        private async Task<ConvertResult[]> ConvertCommand<T>(
            T caller, CommandInfo command, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : ICallerContext
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            args.SetSize(argHeight);

            if (!command.HasArguments && args.Length == 0)
                return [];

            if (command.MaxLength == args.Length)
                return await ParseArguments(caller, command.Arguments, args, options);

            if (command.MaxLength <= args.Length && command.HasRemainder)
                return await ParseArguments(caller, command.Arguments, args, options);

            if (command.MaxLength > args.Length && command.MinLength <= args.Length)
                return await ParseArguments(caller, command.Arguments, args, options);

            return [ConvertResult.FromError(ConvertException.ArgumentMismatch())];
        }

        private async Task<ConvertResult[]> ParseArguments(
            ICallerContext caller, IArgument[] arguments, ArgumentEnumerator args, CommandOptions options)
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var results = new ConvertResult[arguments.Length];

            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];

                if (argument.IsRemainder)
                {
                    results[i] = await argument.Parse(caller, argument.IsCollection ? args.TakeRemaining() : args.JoinRemaining(options.RemainderSeparator), options.Services, options.CancellationToken);

                    break;
                }

                if (argument is ComplexArgumentInfo complexArgument)
                {
                    var result = await ParseArguments(caller, complexArgument.Arguments, args, options);

                    if (result.All(x => x.Success))
                    {
                        try
                        {
                            results[i] = ConvertResult.FromSuccess(complexArgument.Activator.Invoke(caller, null, result.Select(x => x.Value).ToArray(), null, options));
                        }
                        catch (Exception ex)
                        {
                            results[i] = ConvertResult.FromError(ex);
                        }

                        continue;
                    }
                    
                    if (complexArgument.IsOptional)
                        results[i] = ConvertResult.FromSuccess(Type.Missing);

                    continue;
                }

                if (args.TryNext(argument.Name!, out var value))
                {
                    results[i] = await argument.Parse(caller, value, options.Services, options.CancellationToken);

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

        private async Task<ConditionResult> CheckConditions<T>(
            T caller, CommandInfo command, ConditionTrigger trigger, CommandOptions options)
            where T : ICallerContext
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

        private async Task FinalizeInvocation(
            ICallerContext caller, IExecuteResult result, CommandOptions options)
        {
            foreach (var resolver in _handlers)
                await resolver.HandleResult(caller, result, options.Services, options.CancellationToken);
        }

#endregion

        /// <summary>
        ///     Creates a builder that is responsible for setting up all required variables to create, search and run commands from a <see cref="ComponentTree"/>. This builder is pre-configured with default settings.
        /// </summary>
        /// <returns>A new instance of <see cref="ComponentTreeBuilder"/> that builds into a new instance of the <see cref="ComponentTree"/>.</returns>
        public static ITreeBuilder CreateBuilder()
            => new ComponentTreeBuilder();
    }
}
