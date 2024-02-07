using Commands.Core;
using Commands.Exceptions;
using Commands.Reflection;

namespace Commands.Helpers
{
    internal static class ExecutionHelpers
    {
        public static IEnumerable<SearchResult> RecursiveSearch(this IEnumerable<IConditional> components, object[] args, int searchHeight)
        {
            List<SearchResult> discovered = [];

            // select command by name or alias.
            var selection = components.Where(command => command.Aliases.Any(x => x == (string)args[searchHeight]));

            foreach (var component in selection)
            {
                if (component is ModuleInfo module)
                {
                    // add the cluster found in the next iteration, if any.
                    var nested = module.Components.RecursiveSearch(args, searchHeight + 1);
                    discovered.AddRange(nested);
                }
                else
                    // add the top level matches immediately.
                    discovered.Add(new(component as CommandInfo, searchHeight + 1));

                // when the ranges fail, no results should return.
            }

            return discovered;
        }

        public static async Task<ConvertResult[]> RecursiveConvertAsync(this IArgument[] param, ConsumerBase consumer, IServiceProvider services, object[] args, int index, Core.RequestContext context)
        {
            static async ValueTask<ConvertResult> ConvertAsync(IArgument param, ConsumerBase consumer, IServiceProvider services, object arg, RequestContext context)
            {
                if (param.IsNullable && arg is null or "null" or "nothing")
                    return new(value: null);

                if (param.Type == typeof(string) || param.Type == typeof(object))
                    return new(value: arg);

                return await param.Converter.ObjectEvaluateAsync(consumer, param, arg, services, context.CancellationToken);
            }

            var results = new ConvertResult[param.Length];

            for (int i = 0; i < param.Length; i++)
            {
                var parameter = param[i];

                if (parameter.IsRemainder)
                {
                    var input = string.Join(" ", args.Skip(index));
                    if (parameter.Type == typeof(string))
                        results[i] = new(input);
                    else
                        results[i] = await ConvertAsync(parameter, consumer, services, input, context);

                    break;
                }

                if (parameter.IsOptional && args.Length <= index)
                {
                    results[i] = new(Type.Missing);
                    continue;
                }

                if (parameter is ComplexArgumentInfo complex)
                {
                    var result = await complex.Arguments.RecursiveConvertAsync(consumer, services, args, index, context);

                    index += result.Length;

                    if (result.All(x => x.Success()))
                    {
                        try
                        {
                            var obj = complex.Constructor.Invoke(result.Select(x => x.Value).ToArray());
                            results[i] = new(obj);
                        }
                        catch (Exception ex)
                        {
                            results[i] = new(exception: ex);
                        }
                    }
                    continue;
                }

                results[i] = await ConvertAsync(parameter, consumer, services, args[index], context);
                index++;
            }

            return results;
        }
    }
}
