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

            // run through components to discovery queryability
            foreach (var component in components)
            {
                // if module is queryable, it can serve as a top level component.
                if (component.IsQueryable)
                {
                    yield return component;
                }
                // if this is not the case, its subcomponents will be added as top level components.
                else foreach (var subComponent in component.Components)
                    {
                        yield return subComponent;
                    }
            }
        }

        public static IEnumerable<ModuleInfo> BuildComponents(BuildOptions options)
        {
            // run through all defined assemblies.
            foreach (var assembly in options.Assemblies)
            {
                // run through every type of the assembly.
                foreach (var type in assembly.GetTypes())
                {
                    // if the type does not match module type.
                    if (!m_type.IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters || type.IsNested)
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

                        // validate and set aliases.
                        group.ValidateAliases(options.NamingRegex);

                        aliases = group.Aliases;
                    }

                    // yield a new module with or without aliases.
                    yield return new ModuleInfo(type, null, aliases, options);
                }
            }
        }

        public static IEnumerable<ModuleInfo> GetModules(
            ModuleInfo module, BuildOptions options)
        {
            // run through all subtypes of type.
            foreach (var type in module.Type.GetNestedTypes())
            {
                // run through all attributes.
                foreach (var attribute in type.GetCustomAttributes(true))
                {
                    // skip attribute if its not group.
                    if (attribute is not GroupAttribute group)
                    {
                        continue;
                    }

                    // validate aliases.
                    group.ValidateAliases(options.NamingRegex);

                    // yield a new module if all aliases are valid.
                    yield return new ModuleInfo(type, module, group.Aliases, options);
                }
            }
        }

        public static IEnumerable<CommandInfo> GetCommands(ModuleInfo module, BuildOptions options)
        {
            // run through all type methods.
            foreach (var method in module.Type.GetMethods())
            {
                // run through attributes.
                foreach (var attribute in method.GetCustomAttributes(true))
                {
                    // skip attribute if its not command.
                    if (attribute is not CommandAttribute command)
                    {
                        continue;
                    }

                    // validate aliases.
                    command.ValidateAliases(options.NamingRegex);

                    // yield a new command if all aliases are valid.
                    yield return new CommandInfo(module, new ModuleInvoker(method), command.Aliases, false, options);
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

        public static IArgument[] GetParameters(this MethodBase method, bool hasContext, BuildOptions options)
        {
            var parameters = method.GetParameters();

            if (hasContext)
                parameters = parameters.Skip(1).ToArray();

            var arr = new IArgument[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
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
