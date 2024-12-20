using Commands.Conversion;
using System.ComponentModel;
using System.Reflection;

namespace Commands
{
    /// <summary>
    ///     A helper class that exposes utilities for command and module registration.
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
                        names.ValidateAliases(configuration);

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

                    var componentFilter = configuration.GetProperty<Func<IComponent, bool>>("ComponentRegistrationFilter");

                    if (componentFilter?.Invoke(component) ?? true)
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
                        names.ValidateAliases(configuration);

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

                        var componentFilter = configuration.GetProperty<Func<IComponent, bool>>("ComponentRegistrationFilter");

                        if (componentFilter?.Invoke(component) ?? true)
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
        public static IEnumerable<IComponent> GetComponents(ModuleInfo module, ComponentConfiguration configuration)
        {
            if (module.Type == null)
                return [];

            var commands = GetCommands(module.Type, module, module.Aliases.Length > 0, configuration);

            var modules = GetModules(module.Type.GetNestedTypes(), module, true, configuration);

            return commands.Concat(modules);
        }

        /// <summary>
        ///     Returns the type converter for the specified <paramref name="type"/> if it needs to be parsed. Otherwise, returns <see langword="null"/>.
        /// </summary>
        /// <param name="type">The type to get or create a converter for.</param>
        /// <param name="configuration">The configuration which serves as a base from which new converters are </param>
        /// <returns>An instance of <see cref="TypeParser"/> which converts an input into the respective type. <see langword="null"/> if it is a string or object, which does not need to be converted.</returns>
        public static TypeParser? GetParser(Type type, ComponentConfiguration configuration)
        {
            if (!type.IsConvertible())
                return null;

            if (configuration.Parsers.TryGetValue(type, out var converter))
                return converter;

            if (type.IsEnum)
                return EnumParser.GetOrCreate(type);

            if (type.IsArray)
            {
                var elementType = type.GetElementType();

                if (!configuration.Parsers.TryGetValue(elementType!, out converter))
                {
                    if (elementType!.IsString())
                        converter = StringParser.Instance;
                    else if (elementType!.IsObject())
                        converter = ObjectParser.Instance;
                    else
                        throw BuildException.CollectionNotSupported(elementType);
                }

                return ArrayParser.GetOrCreate(converter);
            }

            try
            {
                var elementType = type.GetGenericArguments()[0];

                var enumType = type.GetCollectionType(elementType);

                if (!configuration.Parsers.TryGetValue(elementType, out converter))
                {
                    if (elementType.IsString())
                        converter = StringParser.Instance;
                    else if (elementType.IsObject())
                        converter = ObjectParser.Instance;
                    else
                        throw BuildException.CollectionNotSupported(elementType);
                }

                if (enumType == CollectionType.List)
                    return ListParser.GetOrCreate(converter);

                if (enumType == CollectionType.Set)
                    return SetParser.GetOrCreate(converter);
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
        public static T? GetAttribute<T>(this IScorable component)
            where T : Attribute
            => component.Attributes.GetAttribute<T>();

        internal static T? GetAttribute<T>(this IEnumerable<Attribute> attributes)
            where T : Attribute
            => attributes.OfType<T>().FirstOrDefault();

        internal static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
            => provider.GetCustomAttributes(inherit).OfType<Attribute>();

        internal static bool IsString(this Type type)
            => type.GUID == s_type.GUID;

        internal static bool IsObject(this Type type)
            => type.GUID == o_type.GUID;

        internal static bool IsConvertible(this Type type)
            => type.GUID != o_type.GUID && type.GUID != s_type.GUID;

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
