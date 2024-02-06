using Commands.Core;
using Commands.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Metadata;

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

        public static async Task<ConvertResult[]> RecursiveConvertAsync(this IArgument[] param, ICommandContext context, IServiceProvider services, object[] args, int index, CancellationToken cancellationToken)
        {
            static async ValueTask<ConvertResult> ConvertAsync(IArgument param, ICommandContext context, IServiceProvider services, object arg, CancellationToken cancellationToken)
            {
                if (param.IsNullable && arg is null or "null" or "nothing")
                    return new(arg);

                if (param.Type == typeof(string) || param.Type == typeof(object))
                    return new(arg);

                return await param.Converter.ObjectEvaluateAsync(context, services, param, arg, cancellationToken);
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
                        results[i] = await ConvertAsync(parameter, context, services, input, cancellationToken);

                    break;
                }

                if (parameter.IsOptional && args.Length <= index)
                {
                    results[i] = new(Type.Missing);
                    continue;
                }

                if (parameter is ComplexArgumentInfo complex)
                {
                    var result = await complex.Arguments.RecursiveConvertAsync(context, services, args, index, cancellationToken);

                    index += result.Length;

                    if (result.Any(x => !x.Success()))
                    {
                        try
                        {
                            var obj = complex.Constructor.Invoke(result.Select(x => x.Value).ToArray());
                            results[i] = new(obj);
                        }
                        catch (Exception ex)
                        {
                            results[i] = new(ex);
                        }
                    }
                    continue;
                }

                results[i] = await ConvertAsync(parameter, context, services, args[index], cancellationToken);
                index++;
            }

            return results;
        }
    }
}
