using Commands.Parsing;
using Commands.Reflection;
using System.Collections;
using System.ComponentModel;

namespace Commands
{
    /// <summary>
    ///     A class that exposes command pipeline execution utilities.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ExecutionUtilities
    {
        const string STR_NULL = "null";

        /// <summary>
        ///     Searches for one or more components in a collection of <see cref="ISearchable"/> components.
        /// </summary>
        /// <param name="components">The collection of components to search through.</param>
        /// <param name="args">The arguments used to search.</param>
        /// <param name="searchHeight">The height of the search. This height determines at which point in the argument enumerator, the cursor should be.</param>
        /// <param name="isNested">Determines if the search result is nested, differentiating if there is a lower-level item with a default entity state that should be returned.</param>
        /// <returns>A collection of discovered commands. If no commands were found, this collection is empty.</returns>
        public static IEnumerable<SearchResult> SearchMany(this IEnumerable<ISearchable> components,
            ArgumentEnumerator args, int searchHeight, bool isNested)
        {
            List<SearchResult> discovered = [];

            foreach (var component in components)
            {
                // we should add defaults even if there are more args to resolve.
                if (component.IsDefault && isNested)
                {
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight));
                }

                // if the search is already done, we simply continue and only look for defaults.
                if (args.Length == searchHeight)
                {
                    continue;
                }

                if (!args.TryNext(searchHeight, out var value))
                {
                    continue;
                }

                if (!component.Aliases.Any(x => x == value))
                {
                    continue;
                }

                // if the search found a module, we do inner module checks.
                if (component is ModuleInfo module)
                {
                    // add the cluster found in the next iteration, if any.
                    var nested = module.Components.SearchMany(args, searchHeight + 1, true);
                    discovered.AddRange(nested);

                    // add fallback overloads for module discovery.
                    discovered.Add(SearchResult.FromError(module));
                }
                else
                    // add the top level matches immediately.
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
            }

            return discovered;
        }

        /// <summary>
        ///     Evaluates all preconditions on the specified commands, evaluating them in the respective logical container and returning the result.
        /// </summary>
        /// <typeparam name="T">The consumer of the command that is being executed.</typeparam>
        /// <param name="command">The current command that should be evaluated.</param>
        /// <param name="consumer">The instance of the consumer of the command that is being evaluated.</param>
        /// <param name="options">Options that determine if the conditions should be skipped.</param>
        /// <returns>The result of the evaluation.</returns>
        public static async ValueTask<ConditionResult> EvaluatePreconditions<T>(this CommandInfo command,
            T consumer, CommandOptions options)
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
        ///     Evaluates all postconditions on the specified commands, evaluating them in the respective logical container and returning the result.
        /// </summary>
        /// <typeparam name="T">The consumer of the command that is being executed.</typeparam>
        /// <param name="command">The current command that should be evaluated.</param>
        /// <param name="consumer">The instance of the consumer of the command that is being evaluated.</param>
        /// <param name="options">Options that determine if the conditions should be skipped.</param>
        /// <returns>The result of the evaluation.</returns>
        public static async ValueTask<ConditionResult> EvaluatePostconditions<T>(this CommandInfo command,
            T consumer, CommandOptions options)
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
        ///     Handles the return type of the command, sending the result to the consumer if the return type is not a non-generic task type or void.
        /// </summary>
        /// <typeparam name="T">The consumer of the command that is being executed.</typeparam>
        /// <param name="command">The current command which returned a return value.</param>
        /// <param name="consumer">The instance of the consumer of the command that is being evaluated.</param>
        /// <param name="value">The value returned by the command.</param>
        /// <returns>The result of the return object parsing.</returns>
        public static async ValueTask<InvokeResult> HandleReturnType<T>(this CommandInfo command,
            T consumer, object? value)
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

        internal static async ValueTask<ConvertResult[]> Convert<T>(this CommandInfo command,
            T consumer, int argHeight, ArgumentEnumerator args, CommandOptions options)
            where T : ConsumerBase
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            args.SetSize(argHeight);

            if (command.HasArguments)
            {
                if (command.MaxLength == args.Length)
                {
                    return await command.Arguments.ConvertMany(consumer, args, options);
                }

                if (command.MaxLength <= args.Length && command.HasRemainder)
                {
                    return await command.Arguments.ConvertMany(consumer, args, options);
                }

                if (command.MaxLength > args.Length && command.MinLength <= args.Length)
                {
                    return await command.Arguments.ConvertMany(consumer, args, options);
                }
            }
            else if (args.Length <= 0)
            {
                return [];
            }

            return [ConvertResult.FromError(ConvertException.ArgumentMismatch())];
        }

        internal static async ValueTask<ConvertResult[]> ConvertMany(this IArgument[] arguments,
            ConsumerBase consumer, ArgumentEnumerator args, CommandOptions options)
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            var results = new ConvertResult[arguments.Length];

            // loop all arguments.
            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];

                // parse remainder.
                if (argument.IsRemainder)
                {
                    if (argument.IsCollection)
                    {
                        var remainder = args.TakeRemaining();

                        results[i] = await argument.Convert(consumer, remainder, true, options);
                    }
                    else
                    {
                        var remainder = args.JoinRemaining();

                        results[i] = await argument.Convert(consumer, remainder, false, options);
                    }
                    
                    // End of the line, as remainder is always the last argument.
                    break;
                }

                // parse complex argument.
                if (argument is ComplexArgumentInfo complexArgument)
                {
                    var result = await complexArgument.Arguments.ConvertMany(consumer, args, options);

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
                    {
                        results[i] = ConvertResult.FromSuccess(Type.Missing);
                    }

                    // continue looking for more args
                    continue;
                }

                if (args.TryNext(argument.Name!, out var value))
                {
                    results[i] = await argument.Convert(consumer, value, false, options);

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

        internal static async ValueTask<ConvertResult> Convert(this IArgument argument, ConsumerBase consumer, object? value, bool isArray, CommandOptions options)
        {
            options.CancellationToken.ThrowIfCancellationRequested();

            // if value is nullable and value is null.
            if (argument.IsNullable && value is null or STR_NULL)
                return ConvertResult.FromSuccess();

            if (value is null)
                return ConvertResult.FromError(new ArgumentNullException(argument.Name));

            // if value is string or object.
            if (argument.Type.IsString())
                return ConvertResult.FromSuccess(value?.ToString());

            if (argument.Type.IsObject())
                return ConvertResult.FromSuccess(value);

            // run parser.
            return await argument.Converter!.Evaluate(consumer, argument, value, options.Services, options.CancellationToken);
        }
    }
}
