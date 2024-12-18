using Commands.Conversion;
using System.ComponentModel;
using System.Reflection;

namespace Commands
{
    /// <summary>
    ///     A class that exposes reflection emit utilities for command and module registration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ComponentUtilities
    {
        private static readonly Type m_type = typeof(CommandModule);
        private static readonly Type c_type = typeof(CommandContext<>);

        private static readonly Type o_type = typeof(object);
        private static readonly Type s_type = typeof(string);

        private static readonly Type l_type = typeof(List<>);
        private static readonly Type h_type = typeof(HashSet<>);

        /// <summary>
        ///     Iterates through all assemblies and creates a top-level enumerable with all discovered members that can be directly searched for.
        /// </summary>
        /// <param name="assemblies">The assemblies to iterate through for command discovery.</param>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <returns>A top-level enumerable of all discovered components which can be searched.</returns>
        public static IEnumerable<IComponent> GetTopLevelComponents(Assembly[] assemblies, ComponentConfiguration configuration)
        {
            var modules = GetTopLevelModules(assemblies, configuration);

            // run through components to discovery queryability
            foreach (var module in modules)
            {
                // if module is searchable, it can serve as a top level component.
                if (module.IsSearchable)
                    yield return module;
                // if this is not the case, its subcomponents will be added as top level components.
                else
                {
                    foreach (var subComponent in module)
                        yield return subComponent;
                }
            }
        }

        /// <summary>
        ///     Iterates through all provided assemblies and creates an enumerable of all discovered top-level modules.
        /// </summary>
        /// <param name="assemblies">The assemblies to iterate through for command discovery.</param>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <returns>A top-level enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetTopLevelModules(Assembly[] assemblies, ComponentConfiguration configuration)
        {
            var copy = assemblies.ToArray();

            var arr = new IEnumerable<ModuleInfo>[copy.Length];

            // run through all defined assemblies.
            for (int i = 0; i < copy.Length; i++)
                arr[i] = GetModules(copy[i], configuration);

            return arr.SelectMany(x => x);
        }

        /// <summary>
        ///     Iterates through the types known in the <paramref name="assembly"/> and returns every discovered module.
        /// </summary>
        /// <param name="assembly">The assembly who'se types should be iterated to discover new modules.</param>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <returns>An enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetModules(Assembly assembly, ComponentConfiguration configuration)
            => GetModules(assembly.GetTypes(), null, false, configuration);

        /// <summary>
        ///     Iterates through the types known in the <paramref name="type"/> and returns every discovered module.
        /// </summary>
        /// <param name="type">The types who'se subtypes should be iterated to discover new modules.</param>
        /// <param name="module">The root module of this iteration, if any.</param>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <returns>An enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetModules(Type type, ModuleInfo? module, ComponentConfiguration configuration)
            => GetModules(type.GetNestedTypes(), module, true, configuration);

        /// <summary>
        ///     Iterates through the types known in the <paramref name="types"/> and returns every discovered module.
        /// </summary>
        /// <param name="types">The types that should be iterated to discover new modules.</param>
        /// <param name="module">The root module of this iteration, if any.</param>
        /// <param name="withNested">Determines if the iteration should include nested types.</param>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <returns>An enumerable of all discovered modules.</returns>
        public static IEnumerable<ModuleInfo> GetModules(IEnumerable<Type> types, ModuleInfo? module, bool withNested, ComponentConfiguration configuration)
        {
            foreach (var type in types)
            {
                if (!withNested && type.IsNested)
                    continue;

                if (!m_type.IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
                    continue;

                var aliases = Array.Empty<string>();

                var skip = false;
                // run through all attributes.
                foreach (var attribute in type.GetCustomAttributes(true))
                {
                    if (attribute is NameAttribute names)
                    {
                        // validate aliases.
                        names.ValidateAliases(configuration.NamingPattern);

                        aliases = names.Aliases;
                        continue;
                    }

                    if (attribute is NoBuildAttribute doSkip)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    // yield a new module if all aliases are valid and it shouldn't be skipped.
                    var component = new ModuleInfo(type, module, aliases, configuration);

                    if (configuration.Properties.TryGetValue("ComponentRegistrationFilter", out var filter) && filter is Func<IComponent, bool> componentFilter)
                    {
                        if (componentFilter.Invoke(component))
                            yield return component;
                    }
                    else
                        yield return component;
                }
            }
        }

        /// <summary>
        ///     Iterates through all methods of the <paramref name="type"/> and returns every discovered command.
        /// </summary>
        /// <param name="type">The type who'se methods should be iterated through to discover commands.</param>
        /// <param name="module">The root module of the commands that should be registered.</param>
        /// <param name="withDefaults">Determines if the root module has any defaults to take into consideration.</param>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <returns>An enumerable of all discovered commands.</returns>
        public static IEnumerable<IComponent> GetCommands(Type type, ModuleInfo? module, bool withDefaults, ComponentConfiguration configuration)
        {
            // run through all type methods.

            var members = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            foreach (var member in members)
            {
                var aliases = Array.Empty<string>();

                var skip = false;
                foreach (var attribute in member.GetCustomAttributes(true))
                {
                    if (attribute is NameAttribute names)
                    {
                        names.ValidateAliases(configuration.NamingPattern);

                        aliases = names.Aliases;
                        continue;
                    }

                    if (attribute is NoBuildAttribute doSkip)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip && (withDefaults || aliases.Length > 0))
                {
                    var method = member switch
                    {
                        PropertyInfo property => property.GetMethod,
                        MethodInfo rawMethod => rawMethod,
                        _ => null
                    };

                    if (method != null)
                    {
                        CommandInfo? component;
                        if (method.IsStatic)
                        {
                            var param = method.GetParameters();

                            var hasContext = false;
                            if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == c_type)
                                hasContext = true;

                            component = new CommandInfo(module, new StaticActivator(method, hasContext), aliases, hasContext, configuration);
                        }
                        else
                            component = new CommandInfo(module, new InstanceActivator(method), aliases, false, configuration);

                        if (configuration.Properties.TryGetValue("ComponentRegistrationFilter", out var filter) && filter is Func<IComponent, bool> componentFilter)
                        {
                            if (componentFilter.Invoke(component))
                                yield return component;
                        }
                        else
                            yield return component;
                    }
                }
            }
        }

        /// <summary>
        ///     Iterates through all members of the <paramref name="module"/> and returns every discovered component.
        /// </summary>
        /// <param name="module">The module who'se members should be iterated.</param>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <returns>An array of all discovered components.</returns>
        public static IComponent[] GetComponents(ModuleInfo module, ComponentConfiguration configuration)
        {
            if (module.Type == null)
                return [];

            var commands = GetCommands(module.Type, module, module.Aliases.Length > 0, configuration);

            var modules = GetModules(module.Type.GetNestedTypes(), module, true, configuration);

            return commands.Concat(modules)
                .ToArray();
        }

        /// <summary>
        ///     Returns the type converter for the specified <paramref name="type"/> if it needs to be parsed. Otherwise, returns <see langword="null"/>.
        /// </summary>
        /// <param name="type">The type to get or create a converter for.</param>
        /// <param name="configuration">The configuration which serves as a base from which new converters are </param>
        /// <returns>An instance of <see cref="TypeConverter"/> which converts an input into the respective type. <see langword="null"/> if it is a string or object, which does not need to be converted.</returns>
        public static Conversion.TypeConverter? GetTypeConverter(Type type, ComponentConfiguration configuration)
        {
            if (!type.IsConvertible())
                return null;

            if (configuration.TypeConverters.TryGetValue(type, out var converter))
                return converter;

            if (type.IsEnum)
                return EnumTypeReader.GetOrCreate(type);

            if (type.IsArray)
            {
                var elementType = type.GetElementType();

                if (!configuration.TypeConverters.TryGetValue(elementType!, out converter))
                {
                    if (elementType!.IsString())
                        converter = StringTypeConverter.Instance;
                    else if (elementType!.IsObject())
                        converter = ObjectTypeConverter.Instance;
                    else
                        throw BuildException.CollectionNotSupported(elementType);
                }

                return ArrayTypeConverter.GetOrCreate(converter);
            }

            try
            {
                var elementType = type.GetGenericArguments()[0];

                var enumType = type.GetCollectionType(elementType);

                if (!configuration.TypeConverters.TryGetValue(elementType, out converter))
                {
                    if (elementType.IsString())
                        converter = StringTypeConverter.Instance;
                    else if (elementType.IsObject())
                        converter = ObjectTypeConverter.Instance;
                    else
                        throw BuildException.CollectionNotSupported(elementType);
                }

                if (enumType == CollectionType.List)
                {
                    return ListTypeConverter.GetOrCreate(converter);
                }

                if (enumType == CollectionType.Set)
                {
                    return SetTypeConverter.GetOrCreate(converter);
                }
            }
            catch
            {
                throw BuildException.CollectionNotSupported(type);
            }

            return null;
        }

        /// <summary>
        ///     Gets the first attribute of the specified type set on this command, if it exists.
        /// </summary>
        /// <typeparam name="T">The attribute type to filter by.</typeparam>
        /// <param name="component">The component that should be searched for the attribute.</param>
        /// <returns>An attribute of the type <typeparamref name="T"/> if it exists, otherwise <see langword="null"/>.</returns>
        public static T? GetAttribute<T>(this IScoreable component)
            where T : Attribute
        {
            return component.Attributes.GetAttribute<T>();
        }

        internal static bool IsString(this Type type)
        {
            return type.GUID == s_type.GUID;
        }

        internal static bool IsObject(this Type type)
        {
            return type.GUID == o_type.GUID;
        }

        internal static bool IsConvertible(this Type type)
        {
            return type.GUID != o_type.GUID && type.GUID != s_type.GUID;
        }

        internal static CollectionType GetCollectionType(this Type type, Type? elementType = null)
        {
            if (type.IsArray)
                return CollectionType.Array;

            if (elementType != null)
            {
                if (type.IsAssignableFrom(l_type.MakeGenericType(elementType)))
                    return CollectionType.List;

                if (type.IsAssignableFrom(h_type.MakeGenericType(elementType)))
                    return CollectionType.Set;
            }

            return CollectionType.None;
        }

        internal static IArgument[] GetArguments(this MethodBase method, bool withContext, ComponentConfiguration configuration)
        {
            var parameters = method.GetParameters();

            if (withContext)
                parameters = parameters.Skip(1).ToArray();

            var arr = new IArgument[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var complex = false;
                var name = string.Empty;
                foreach (var attr in parameters[i].GetCustomAttributes())
                {
                    if (attr is ComplexAttribute)
                        complex = true;

                    if (attr is NameAttribute names)
                    {
                        // aliases is not supported for parameters.
                        name = names.Name;
                    }
                }

                if (complex)
                    arr[i] = new ComplexArgumentInfo(parameters[i], name, configuration);
                else
                    arr[i] = new ArgumentInfo(parameters[i], name, configuration);
            }

            return arr;
        }

        internal static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
            => provider.GetCustomAttributes(inherit).OfType<Attribute>();

        internal static T? GetAttribute<T>(this IEnumerable<Attribute> attributes)
            where T : Attribute
            => attributes.OfType<T>().FirstOrDefault();

        internal static bool ContainsAttribute<T>(this IEnumerable<Attribute> attributes, bool allowMultipleMatches)
        {
            var found = false;
            foreach (var entry in attributes)
            {
                if (entry is T)
                {
                    if (!allowMultipleMatches)
                    {
                        if (!found)
                            found = true;
                        else
                            return false;
                    }
                    else
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
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
