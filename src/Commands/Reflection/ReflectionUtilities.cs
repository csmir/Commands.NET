using Commands.Converters;
using Commands.Helpers;
using System.ComponentModel;
using System.Reflection;

namespace Commands.Reflection
{
    /// <summary>
    ///     A class that exposes reflection emit utilities for command and module registration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ReflectionUtilities
    {
        private static readonly Type m_type = typeof(ModuleBase);
        private static readonly Type c_type = typeof(CommandContext<>);

        private static readonly Type o_type = typeof(object);
        private static readonly Type s_type = typeof(string);

        private static readonly Type l_type = typeof(List<>);
        private static readonly Type h_type = typeof(HashSet<>);

        /// <summary>
        ///     Iterates through all assemblies registered in <paramref name="options"/> and creates a top-level enumerable with all discovered members that can be directly searched for.
        /// </summary>
        /// <param name="options">The options that define the command registration process.</param>
        /// <returns>A top-level enumerable of all discovered components which can be searched.</returns>
        public static IEnumerable<ISearchable> GetTopLevelComponents(CommandConfiguration options)
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
        public static IEnumerable<ModuleInfo> GetTopLevelModules(CommandConfiguration options)
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
        public static IEnumerable<ModuleInfo> GetModules(Assembly assembly, CommandConfiguration options)
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
        public static IEnumerable<ModuleInfo> GetModules(Type type, ModuleInfo? module, CommandConfiguration options)
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
        public static IEnumerable<ModuleInfo> GetModules(IEnumerable<Type> types, ModuleInfo? module, bool withNested, CommandConfiguration options)
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
        public static IEnumerable<ISearchable> GetCommands(Type type, ModuleInfo? module, bool withDefaults, CommandConfiguration options)
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
                    var method = member switch
                    {
                        PropertyInfo property => property.GetMethod,
                        MethodInfo rawMethod => rawMethod,
                        _ => null
                    };

                    if (method != null)
                    {
                        if (method.IsStatic)
                        {
                            var param = method.GetParameters();

                            var hasContext = false;
                            if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == c_type)
                            {
                                hasContext = true;
                            }

                            yield return new CommandInfo(module, new StaticInvoker(method, hasContext), aliases, hasContext, options);
                        }
                        else
                        {
                            yield return new CommandInfo(module, new InstanceInvoker(method), aliases, false, options);
                        }
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
        public static ISearchable[] GetComponents(ModuleInfo module, CommandConfiguration options)
        {
            var commands = GetCommands(module.Type, module, module.Aliases.Length > 0, options);

            var modules = GetModules(module.Type.GetNestedTypes(), module, true, options);

            return commands.Concat(modules)
                .ToArray();
        }

        /// <summary>
        ///     Returns the type converter for the specified <paramref name="type"/> if it needs to be parsed. Otherwise, returns <see langword="null"/>.
        /// </summary>
        /// <param name="type">The type to get or create a converter for.</param>
        /// <param name="options">The options which serves as a base from which new converters are </param>
        /// <returns>An instance of <see cref="TypeConverterBase"/> which converts an input into the respective type. <see langword="null"/> if it is a string or object, which does not need to be converted.</returns>
        public static TypeConverterBase? GetTypeConverter(Type type, CommandConfiguration options)
        {
            if (!type.IsConvertible())
            {
                return null;
            }

            if (options.TypeConverters.TryGetValue(type, out var converter))
            {
                return converter;
            }

            if (type.IsEnum)
            {
                return EnumTypeReader.GetOrCreate(type);
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();

                if (!options.TypeConverters.TryGetValue(elementType!, out converter))
                {
                    if (elementType!.IsString())
                        converter = StringTypeConverter.Instance;
                    else if (elementType!.IsObject())
                        converter = ObjectTypeConverter.Instance;
                    else
                        ThrowHelpers.ThrowInvalidOperation("The inner type of this generic argument is not supported for conversion. Add a TypeConverter to the ConfigurationBuilder to support this type.");
                }

                return ArrayTypeConverter.GetOrCreate(converter);
            }

            try
            {
                var elementType = type.GetGenericArguments()[0];

                var enumType = type.GetCollectionType(elementType);

                if (!options.TypeConverters.TryGetValue(elementType, out converter))
                {
                    if (elementType.IsString())
                        converter = StringTypeConverter.Instance;
                    else if (elementType.IsObject())
                        converter = ObjectTypeConverter.Instance;
                    else
                        ThrowHelpers.ThrowInvalidOperation("The inner type of this generic argument is not supported for conversion. Add a TypeConverter to the ConfigurationBuilder to support this type.");
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
                ThrowHelpers.ThrowInvalidOperation($"Type {type.FullName} is not supported for conversion. Add a TypeConverter to the ConfigurationBuilder to support this type. ");
            }

            return null;
        }

        /// <summary>
        ///     Gets the first attribute of the specified type set on this command, if it exists.
        /// </summary>
        /// <typeparam name="T">The attribute type to filter by.</typeparam>
        /// <param name="component">The component that should be searched for the attribute.</param>
        /// <param name="defaultValue">The default value that will be returned if an attribute was not found. <see langword="default"/> if not set.</param>
        /// <returns>An attribute of the type <typeparamref name="T"/> if it exists, otherwise <paramref name="defaultValue"/>.</returns>
        public static T? GetAttribute<T>(this IScoreable component, T? defaultValue = default)
            where T : Attribute
        {
            return component.Attributes.GetAttribute(defaultValue);
        }

        internal static bool IsString(this Type type)
        {
            return type.FullName == s_type.FullName;
        }

        internal static bool IsObject(this Type type)
        {
            return type.FullName == o_type.FullName;
        }

        internal static bool IsConvertible(this Type type)
        {
            return type != o_type && type != s_type;
        }

        internal static CollectionType GetCollectionType(this Type type, Type? elementType = null)
        {
            if (type.IsArray)
            {
                return CollectionType.Array;
            }

            if (elementType != null)
            {
                if (l_type.MakeGenericType(elementType).IsAssignableTo(type))
                {
                    return CollectionType.List;
                }

                if (h_type.MakeGenericType(elementType).IsAssignableTo(type))
                {
                    return CollectionType.Set;
                }
            }

            return CollectionType.None;
        }

        internal static IArgument[] GetArguments(this MethodBase method, bool withContext, CommandConfiguration options)
        {
            var parameters = method.GetParameters();

            if (withContext)
            {
                parameters = parameters.Skip(1).ToArray();
            }

            var arr = new IArgument[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var complex = false;
                var name = string.Empty;
                foreach (var attr in parameters[i].GetCustomAttributes())
                {
                    if (attr is ComplexAttribute)
                    {
                        complex = true;
                    }

                    if (attr is NameAttribute names)
                    {
                        // aliases is not supported for parameters.
                        name = names.Name;
                    }
                }

                if (complex)
                {
                    arr[i] = new ComplexArgumentInfo(parameters[i], name, options);
                }
                else
                {
                    arr[i] = new ArgumentInfo(parameters[i], name, options);
                }
            }

            return arr;
        }

        internal static IParameter[] GetParameters(this MethodBase method, CommandConfiguration _)
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
            => provider.GetCustomAttributes(inherit).OfType<Attribute>();

        internal static T? GetAttribute<T>(this IEnumerable<Attribute> attributes, T? defaultValue = null)
            where T : Attribute
            => attributes.OfType<T>().FirstOrDefault(defaultValue);

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
