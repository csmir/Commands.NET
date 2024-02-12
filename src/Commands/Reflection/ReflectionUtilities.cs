using Commands.Conditions;
using Commands.Helpers;
using Commands.Reflection;
using Commands.TypeConverters;
using System.ComponentModel;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     Exposes reflection emit utilities for command and module registration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class ReflectionUtilities
    {
        private static readonly Type m_type = typeof(ModuleBase);

        /// <summary>
        ///     Iterates through all assemblies registered in <paramref name="options"/> and creates a top-level enumerable with all discovered members that can be directly searched for.
        /// </summary>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>A top-level enumerable of all discovered components which can be searched.</returns>
        public static IEnumerable<ISearchable> GetTopLevelComponents(ICommandBuilder options)
        {
            var modules = GetTopLevelModules(options);

            // run through components to discovery queryability
            foreach (var module in modules)
            {
                // if module is searchable, it can serve as a top level component.
                if (module.IsSearchable)
                {
                    yield return module;
                }
                // if this is not the case, its subcomponents will be added as top level components.
                else
                {
                    foreach (var subComponent in module.Components)
                    {
                        yield return subComponent;
                    }
                }
            }
        }

        /// <summary>
        ///     Iterates through all assemblies registered in <paramref name="options"/> and creates an enumerable of all discovered top-level modules.
        /// </summary>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>A top-level enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetTopLevelModules(ICommandBuilder options)
        {
            var arr = new IEnumerable<ModuleInfo>[options.Assemblies.Count];

            // run through all defined assemblies.
            for (int i = 0; i < options.Assemblies.Count; i++)
            {
                arr[i] = GetModules(options.Assemblies[i], options);
            }

            return arr.SelectMany(x => x);
        }

        /// <summary>
        ///     Iterates through the types known in the <paramref name="assembly"/> and returns every discovered module.
        /// </summary>
        /// <param name="assembly">The assembly who'se types should be iterated to discover new modules.</param>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>An enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetModules(Assembly assembly, ICommandBuilder options)
        {
            return GetModules(assembly.GetTypes(), null, false, options);
        }

        /// <summary>
        ///     Iterates through the types known in the <paramref name="type"/> and returns every discovered module.
        /// </summary>
        /// <param name="type">The types who'se subtypes should be iterated to discover new modules.</param>
        /// <param name="module">The root module of this iteration, if any.</param>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>An enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetModules(Type type, ModuleInfo? module, ICommandBuilder options)
        {
            return GetModules(type.GetNestedTypes(), module, true, options);
        }

        /// <summary>
        ///     Iterates through the types known in the <paramref name="types"/> and returns every discovered module.
        /// </summary>
        /// <param name="types">The types that should be iterated to discover new modules.</param>
        /// <param name="module">The root module of this iteration, if any.</param>
        /// <param name="withNested">Determines if the iteration should include nested types.</param>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>An enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetModules(IEnumerable<Type> types, ModuleInfo? module, bool withNested, ICommandBuilder options)
        {
            foreach (var type in types)
            {
                if (!withNested && type.IsNested)
                {
                    continue;
                }

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

                    if (attribute is SkipAttribute doSkip)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    // yield a new module if all aliases are valid and it shouldn't be skipped.
                    yield return new ModuleInfo(type, module, aliases, options);
                }
            }
        }

        /// <summary>
        ///     Iterates through all methods of the <paramref name="type"/> and returns every discovered command.
        /// </summary>
        /// <param name="type">The type who'se methods should be iterated through to discover commands.</param>
        /// <param name="module">The root module of the commands that should be registered.</param>
        /// <param name="withDefaults">Determines if the root module has any defaults to take into consideration.</param>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>An enumerable of all discovered commands.</returns>
        public static IEnumerable<ISearchable> GetCommands(Type type, ModuleInfo? module, bool withDefaults, ICommandBuilder options)
        {
            // run through all type methods.

            var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

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

                    if (attribute is SkipAttribute doSkip)
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

        /// <summary>
        ///     Iterates through all members of the <paramref name="module"/> and returns every discovered component.
        /// </summary>
        /// <param name="module">The module who'se members should be iterated.</param>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>An array of all discovered components.</returns>
        public static ISearchable[] GetComponents(ModuleInfo module, ICommandBuilder options)
        {
            var commands = GetCommands(module.Type, module, module.IsDefault, options);

            var modules = GetModules(module.Type.GetNestedTypes(), module, true, options);

            return commands.Concat(modules)
                .ToArray();
        }

        internal static IArgument[] GetArguments(this MethodBase method, bool hasContext, ICommandBuilder options)
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

        internal static IParameter[] GetParameters(this MethodBase method, ICommandBuilder _)
        {
            var parameters = method.GetParameters();

            var arr = new IParameter[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                arr[i] = new ServiceInfo(parameters[i]);
            }

            return arr;
        }

        internal static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
        {
            return provider.GetCustomAttributes(inherit)
                .CastWhere<Attribute>();
        }

        internal static Tuple<int, int> GetLength(this IArgument[] parameters)
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
