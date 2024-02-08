using Commands.Core;
using Commands.Reflection;

namespace Commands.Helpers
{
    internal static class ExecutionHelpers
    {
        const string STR_NULL = "null";
        const string STR_WHITESPACE = " ";

        private static readonly Type o_type = typeof(object);
        private static readonly Type s_type = typeof(string);

        public static IEnumerable<SearchResult> SearchMany(this IEnumerable<IConditional> components,
            object[] args, int searchHeight)
        {
            List<SearchResult> discovered = [];

            if (args.Length == searchHeight)
            {
                return discovered;
            }

            // select command by name or alias.
            var selection = components.Where(command => command.Aliases.Any(x => x == (string)args[searchHeight]));

            foreach (var component in selection)
            {
                if (component is ModuleInfo module)
                {
                    // add the cluster found in the next iteration, if any.
                    var nested = module.Components.SearchMany(args, searchHeight + 1);
                    discovered.AddRange(nested);

                    // add fallback overloads for module discovery.
                    discovered.Add(new(module));
                }
                else
                    // add the top level matches immediately.
                    discovered.Add(new(component, searchHeight + 1));
            }

            return discovered;
        }

        public static async Task<ConvertResult[]> ConvertManyAsync(this IArgument[] arguments,
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
                    var input = string.Join(STR_WHITESPACE, args.Skip(index));
                    if (argument.Type == s_type)
                    {
                        results[i] = new(input);
                    }
                    else
                    {
                        results[i] = await argument.ConvertAsync(consumer, input, options);
                    }

                    // end loop as remainder is last param.
                    break;
                }

                // parse missing optional argument.
                if (argument.IsOptional && args.Length <= index)
                {
                    results[i] = new(Type.Missing);

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
                            var obj = complexArgument.Constructor.Invoke(result.Select(x => x.Value).ToArray());
                            results[i] = new(obj);
                        }
                        catch (Exception ex)
                        {
                            results[i] = new(exception: ex);
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
                return new(value: null);

            // if value is string or object.
            if (argument.Type == s_type || argument.Type == o_type)
                return new(value: value);

            // run parser.
            return await argument.Converter.EvaluateAsync(consumer, argument, value.ToString(), options.Scope.ServiceProvider, options.CancellationToken);
        }
    }
}
