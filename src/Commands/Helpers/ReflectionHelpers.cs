using Commands.Conditions;
using Commands.Core;
using Commands.Reflection;
using Commands.TypeConverters;
using System.Reflection;

namespace Commands.Helpers
{
    internal static class ReflectionHelpers
    {
        private static readonly Type m_type = typeof(ModuleBase);

        public static IEnumerable<IConditional> BuildComponents(IEnumerable<TypeConverterBase> converters, BuildOptions options)
        {
            options.SetKeyedConverters(converters);

            var components = BuildComponents(options);

            foreach (var component in components)
            {
                if (component.IsQueryable)
                {
                    yield return component;
                }
                else foreach (var subComponent in component.Components)
                {
                    yield return subComponent;
                }
            }
        }

        public static IEnumerable<ModuleInfo> BuildComponents(BuildOptions options)
        {
            foreach (var assembly in options.Assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    // if the type does not match module type.
                    if (!m_type.IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
                    {
                        continue;
                    }

                    var aliases = Array.Empty<string>();

                    // search all attributes for occurrence of group, in which case we validate aliases
                    foreach (var attribute in type.GetCustomAttributes(true))
                    {
                        // if attribute is not group, we can skip it.
                        if (attribute is not GroupAttribute group)
                        {
                            continue;
                        }

                        group.ValidateAliases(options.NamingRegex);

                        aliases = group.Aliases;
                    }

                    yield return new ModuleInfo(type, null, aliases, options);
                }
            }
        }

        public static IEnumerable<ModuleInfo> GetModules(
            ModuleInfo module, BuildOptions options)
        {
            foreach (var type in module.Type.GetNestedTypes())
            {
                foreach (var attribute in type.GetCustomAttributes(true))
                {
                    if (attribute is not GroupAttribute group)
                    {
                        continue;
                    }

                    group.ValidateAliases(options.NamingRegex);

                    yield return new ModuleInfo(type, module, group.Aliases, options);
                }
            }
        }

        public static IEnumerable<CommandInfo> GetCommands(ModuleInfo module, BuildOptions options)
        {
            foreach (var method in module.Type.GetMethods())
            {
                var aliases = Array.Empty<string>();

                foreach (var attribute in method.GetCustomAttributes(true))
                {
                    if (attribute is not CommandAttribute command)
                    {
                        continue;
                    }

                    command.ValidateAliases(options.NamingRegex);

                    yield return new CommandInfo(module, method, command.Aliases, options);
                }
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
