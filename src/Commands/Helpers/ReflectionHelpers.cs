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

                    var skip = false;
                    // search all attributes for occurrence of group, in which case we validate aliases
                    foreach (var attribute in type.GetCustomAttributes(true))
                    {
                        // if attribute is not group, we can skip it.
                        if (attribute is not NameAttribute names)
                        {
                            continue;
                        }

                        if (attribute is DoNotRegister doSkip)
                        {
                            skip = true;
                            break;
                        }

                        // validate and set aliases.
                        names.ValidateAliases(options.NamingRegex);

                        aliases = names.Aliases;
                    }

                    if (!skip)
                    {
                        // yield a new module with or without aliases.
                        yield return new ModuleInfo(type, null, aliases, options);
                    }
                }
            }
        }

        public static IEnumerable<ModuleInfo> GetModules(
            ModuleInfo module, BuildOptions options)
        {
            // run through all subtypes of type.
            foreach (var type in module.Type.GetNestedTypes())
            {
                if (!m_type.IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
                {
                    continue;
                }

                var aliases = Array.Empty<string>();

                var skip = false;
                // run through all attributes.
                foreach (var attribute in type.GetCustomAttributes(true))
                {
                    if (attribute is NameAttribute names)
                    {
                        // validate aliases.
                        names.ValidateAliases(options.NamingRegex);

                        aliases = names.Aliases;
                        continue;
                    }

                    if (attribute is DoNotRegister doSkip)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip && aliases.Length != 0)
                {
                    // yield a new module if all aliases are valid and it shouldn't be skipped.
                    yield return new ModuleInfo(type, module, aliases, options);
                }
            }
        }

        public static IEnumerable<CommandInfo> GetCommands(ModuleInfo module, bool withDefaults, BuildOptions options)
        {
            // run through all type methods.

            var methods = module.Type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            foreach (var method in methods)
            {
                var aliases = Array.Empty<string>();

                var skip = false;
                foreach (var attribute in method.GetCustomAttributes(true))
                {
                    if (attribute is NameAttribute names)
                    {
                        names.ValidateAliases(options.NamingRegex);

                        aliases = names.Aliases;
                        continue;
                    }

                    if (attribute is DoNotRegister doSkip)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip && (withDefaults || aliases.Length > 0))
                {
                    if (method.IsStatic)
                    {
                        yield return new CommandInfo(module, new StaticInvoker(method), aliases, true, options);
                    }
                    else
                    {
                        yield return new CommandInfo(module, new InstanceInvoker(method), aliases, false, options);
                    }
                }
            }
        }

        public static IConditional[] GetComponents(this ModuleInfo module, bool withDefaults, BuildOptions options)
        {
            var commands = (IEnumerable<IConditional>)GetCommands(module, withDefaults, options)
                .OrderBy(x => x.Arguments.Length);

            var modules = (IEnumerable<IConditional>)GetModules(module, options)
                .OrderBy(x => x.Components.Count);

            return commands.Concat(modules)
                .ToArray();
        }

        public static IArgument[] GetParameters(this MethodBase method, bool hasContext, BuildOptions options)
        {
            var parameters = method.GetParameters();

            if (hasContext)
            {
                if (parameters.Length == 0)
                {
                    ThrowHelpers.ThrowInvalidOperation($"A delegate or static command signature must implement {nameof(CommandContext)} as the first parameter.");
                }

                parameters = parameters.Skip(1).ToArray();
            }

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
