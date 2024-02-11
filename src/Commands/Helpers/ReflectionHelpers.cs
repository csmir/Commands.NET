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

        public static IEnumerable<IConditional> GetTopLevelComponents(IEnumerable<TypeConverterBase> converters, BuildOptions options)
        {
            options.SetKeyedConverters(converters);

            var modules = GetTopLevelModules(options);

            // run through components to discovery queryability
            foreach (var module in modules)
            {
                // if module is queryable, it can serve as a top level component.
                if (module.IsQueryable)
                {
                    yield return module;
                }
                // if this is not the case, its subcomponents will be added as top level components.
                else
                {
                    foreach (var subComponent in (module as ModuleInfo)!.Components)
                    {
                        yield return subComponent;
                    }
                }
            }
        }

        public static IEnumerable<IConditional> GetTopLevelModules(BuildOptions options)
        {
            var arr = new IEnumerable<IConditional>[options.Assemblies.Count];

            // run through all defined assemblies.
            for (int i = 0; i < options.Assemblies.Count; i++)
            {
                arr[i] = GetModules(options.Assemblies[i].GetTypes().Where(x => !x.IsNested), null, options);
            }

            return arr.SelectMany(x => x);
        }

        public static IEnumerable<IConditional> GetModules(IEnumerable<Type> types,
            ModuleInfo? module, BuildOptions options)
        {
            // run through all subtypes of type.
            foreach (var type in types)
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

        public static IEnumerable<IConditional> GetCommands(ModuleInfo module, bool withDefaults, BuildOptions options)
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

        public static IConditional[] GetComponents(ModuleInfo module, bool withDefaults, BuildOptions options)
        {
            var commands = GetCommands(module, withDefaults, options);

            var modules = GetModules(module.Type.GetNestedTypes(), module, options);

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

        public static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
        {
            return provider.GetCustomAttributes(inherit)
                .CastWhere<Attribute>();
        }

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
