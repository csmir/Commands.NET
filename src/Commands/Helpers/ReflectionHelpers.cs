using Commands.Conditions;
using Commands.Core;
using Commands.Reflection;
using Commands.TypeConverters;
using System.Reflection;

namespace Commands.Helpers
{
    internal static class ReflectionHelpers
    {
        public static IEnumerable<ModuleInfo> BuildComponents(
            IEnumerable<TypeConverterBase> converters, BuildOptions options)
        {
            options.SetKeyedConverters(converters);

            var rootType = typeof(ModuleBase);
            foreach (var assembly in options.Assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (rootType.IsAssignableFrom(type)
                        && !type.IsAbstract
                        && !type.ContainsGenericParameters)
                    {
                        var aliases = Array.Empty<string>();

                        foreach (var attribute in type.GetCustomAttributes(true))
                        {
                            if (attribute is GroupAttribute gattribute)
                            {
                                foreach (var alias in gattribute.Aliases)
                                {
                                    if (!options.NamingRegex.IsMatch(alias))
                                    {
                                        if (options.ThrowOnMatchFailure)
                                        {
                                            ThrowHelpers.ThrowInvalidNaming(alias);
                                        }

                                        continue;
                                    }
                                }

                                aliases = gattribute.Aliases;
                            }
                        }

                        yield return new ModuleInfo(type, null, aliases, options);
                    }
                }
            }
        }

        public static IEnumerable<ModuleInfo> GetModules(
            ModuleInfo module, BuildOptions options)
        {
            foreach (var group in module.Type.GetNestedTypes())
            {
                foreach (var attribute in group.GetCustomAttributes(true))
                {
                    if (attribute is GroupAttribute gattribute)
                    {
                        foreach (var alias in gattribute.Aliases)
                        {
                            if (!options.NamingRegex.IsMatch(alias))
                            {
                                if (options.ThrowOnMatchFailure)
                                {
                                    ThrowHelpers.ThrowInvalidNaming(alias);
                                }

                                continue;
                            }
                        }

                        yield return new ModuleInfo(group, module, gattribute.Aliases, options);
                    }
                }
            }
        }

        public static IEnumerable<CommandInfo> GetCommands(ModuleInfo module, BuildOptions options)
        {
            foreach (var method in module.Type.GetMethods())
            {
                var attributes = method.GetCustomAttributes(true);

                string[] aliases = [];
                foreach (var attribute in attributes)
                {
                    if (attribute is CommandAttribute cmd)
                    {
                        foreach (var alias in cmd.Aliases)
                        {
                            if (!options.NamingRegex.IsMatch(alias))
                            {
                                if (options.ThrowOnMatchFailure)
                                {
                                    ThrowHelpers.ThrowInvalidNaming(alias);
                                }

                                continue;
                            }
                        }

                        aliases = cmd.Aliases;
                    }
                }

                if (aliases.Length == 0)
                {
                    continue;
                }

                yield return new CommandInfo(module, method, aliases, options);
            }
        }

        public static IConditional[] GetComponents(this ModuleInfo module, BuildOptions options)
        {
            var commands = (IEnumerable<IConditional>)GetCommands(module, options)
                .OrderBy(x => x.Arguments.Length);

            var modules = (IEnumerable<IConditional>)GetModules(module, options)
                .OrderBy(x => x.Components.Length);

            return commands.Concat(modules)
                .ToArray();
        }

        public static IArgument[] GetParameters(this MethodBase method, BuildOptions options)
        {
            var parameters = method.GetParameters();

            var arr = new IArgument[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].GetCustomAttributes().Any(x => x is ComplexAttribute))
                {
                    arr[i] = new ComplexArgumentInfo(parameters[i], options);
                }
                else
                {
                    arr[i] = new ArgumentInfo(parameters[i], options);
                }
            }

            return arr;
        }

        public static PreconditionAttribute[] GetPreconditions(this Attribute[] attributes)
            => attributes.CastWhere<PreconditionAttribute>().ToArray();

        public static PostconditionAttribute[] GetPostconditions(this Attribute[] attributes)
            => attributes.CastWhere<PostconditionAttribute>().ToArray();

        public static Attribute[] GetAttributes(this ICustomAttributeProvider provider, bool inherit)
            => provider.GetCustomAttributes(inherit).CastWhere<Attribute>().ToArray();

        public static Tuple<int, int> GetLength(this IArgument[] parameters)
        {
            var minLength = 0;
            var maxLength = 0;

            foreach (var parameter in parameters)
            {
                if (parameter is ComplexArgumentInfo complexParam)
                {
                    maxLength += complexParam.MaxLength;
                    minLength += complexParam.MinLength;
                }

                if (parameter is ArgumentInfo defaultParam)
                {
                    maxLength++;
                    if (!defaultParam.IsOptional)
                        minLength++;
                }
            }

            return new(minLength, maxLength);
        }
    }
}
