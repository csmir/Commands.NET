using Commands.Reflection;

namespace Commands.Helpers
{
    internal static class ExecutionHelpers
    {
        const string STR_NULL = "null";
        const string STR_WHITESPACE = " ";

        private static readonly Type o_type = typeof(object);
        private static readonly Type s_type = typeof(string);

        public static IEnumerable<SearchResult> SearchMany(this IEnumerable<ISearchable> components,
            object[] args, int searchHeight, bool isGrouped)
        {
            List<SearchResult> discovered = [];

            foreach (var component in components)
            {
                // we should add defaults even if there are more args to resolve.
                if (component.IsDefault && isGrouped)
                {
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight));
                }

                // if the search is already done, we simply continue and only look for defaults.
                if (args.Length == searchHeight)
                {
                    continue;
                }

                // if no aliases match the current arg, we skip at this point.
                if (!component.Aliases.Any(x => x == (string)args[searchHeight]))
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

        public static async ValueTask<ConvertResult[]> ConvertManyAsync(this IArgument[] arguments,
            ConsumerBase consumer, object[] args, int index, CommandOptions options)
        {
            var results = new ConvertResult[arguments.Length];

            // loop all arguments.
            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = arguments[i];

                // parse remainder.
                if (argument.IsRemainder)
                {
                    var remainder = string.Join(STR_WHITESPACE, args.Skip(index));
                    if (argument.Type == s_type)
                    {
                        results[i] = ConvertResult.FromSuccess(remainder);
                    }
                    else
                    {
                        results[i] = await argument.ConvertAsync(consumer, remainder, options);
                    }

                    // end loop as remainder is last param.
                    break;
                }

                // parse missing optional argument.
                if (argument.IsOptional && args.Length <= index)
                {
                    results[i] = ConvertResult.FromSuccess(Type.Missing);

                    // continue looking for more optionals.
                    continue;
                }

                // parse complex argument.
                if (argument is ComplexArgumentInfo complexArgument)
                {
                    var result = await complexArgument.Arguments.ConvertManyAsync(consumer, args, index, options);

                    index += result.Length;

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
                    }

                    // continue looking for more args
                    continue;
                }

                // default, non-exclusive agrument
                results[i] = await argument.ConvertAsync(consumer, args[index], options);

                index++;
            }

            return results;
        }

        public static async ValueTask<ConvertResult> ConvertAsync(this IArgument argument, ConsumerBase consumer, object value, CommandOptions options)
        {
            // if value is nullable and value is null.
            if (argument.IsNullable && value is STR_NULL)
                return ConvertResult.FromSuccess();

            // if value is string or object.
            if (argument.Type == s_type || argument.Type == o_type)
                return ConvertResult.FromSuccess(value);

            // run parser.
            return await argument.Converter!.EvaluateAsync(consumer, argument, value.ToString(), options.Services, options.CancellationToken);
        }
    }
}
