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
        /// <summary>
        ///     Browses through the types known in the provided <paramref name="assemblies"/> and returns every discovered top-level module.
        /// </summary>
        /// <param name="configuration">The configuration that defines the command registration process.</param>
        /// <param name="assemblies">The assemblies that should be searched to discover new modules.</param>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> containing all discovered modules in the provided <paramref name="assemblies"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<ModuleInfo> GetComponents(this ComponentConfiguration configuration, params Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            return configuration.GetComponents(assemblies.SelectMany(x => x.GetTypes()).ToArray());
        }

        /// <summary>
        ///     Browses through the types known in the <paramref name="types"/> and returns every discovered top-level module.
        /// </summary>
        /// <param name="configuration">The configuration that defines the command registration process.</param>
        /// <param name="types">The types that should be searched to discover new modules.</param>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> containing all discovered modules in the provided <paramref name="types"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<ModuleInfo> GetComponents(this ComponentConfiguration configuration, params Type[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            return configuration.GetModules(types, null, false);
        }

        /// <summary>
        ///     Iterates through all members of the <paramref name="parent"/> and returns every discovered component.
        /// </summary>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <param name="parent">The module who'se members should be iterated.</param>
        /// <returns>An array of all discovered components.</returns>
        public static IEnumerable<IComponent> GetNestedComponents(this ComponentConfiguration configuration, ModuleInfo parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (parent.Type == null)
                return [];

            var commands = configuration.GetCommands(parent.Type, parent, parent.Aliases.Length > 0);

            var modules = configuration.GetModules(parent.Type.GetNestedTypes(), parent, true);

            return commands.Concat(modules);
        }

        /// <summary>
        ///     Returns the parser for the specified <paramref name="type"/> if it needs to be parsed. Otherwise, returns <see langword="null"/>.
        /// </summary>
        /// <param name="configuration">The configuration that define the command registration process.</param>
        /// <param name="type">The type to get or create a parser for.</param>
        /// <returns>An instance of <see cref="TypeParser"/> which converts an input into the respective type. <see langword="null"/> if it is a string or object and no custom converter is defined, which do not need to be converted.</returns>
        public static TypeParser? GetParser(this ComponentConfiguration configuration, Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            TypeParser GetParser(Type elementType)
            {
                if (!configuration.Parsers.TryGetValue(elementType!, out var parser))
                {
                    if (elementType.IsEnum)
                        return EnumParser.GetOrCreate(elementType);

                    // csmir: Chosen not to support nested collections as this is a whole different level of complexity for both parsing and validation.

                    throw BuildException.CollectionNotSupported(elementType);
                }

                return parser;
            }

            if (configuration.Parsers.TryGetValue(type, out var converter))
                return converter;

            if (type.IsEnum)
                return EnumParser.GetOrCreate(type);

            if (type.IsArray)
                return ArrayParser.GetOrCreate(GetParser(type.GetElementType()));

            try
            {
                var elementType = type.GetGenericArguments()[0];

                converter = GetParser(elementType);

                var enumType = type.GetCollectionType(elementType);

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
        ///     Gets the first attribute of the specified type set on this component, if it exists.
        /// </summary>
        /// <typeparam name="T">The attribute type to filter by.</typeparam>
        /// <param name="component">The component that should be searched for the attribute.</param>
        /// <returns>An attribute of the type <typeparamref name="T"/> if it exists; Otherwise <see langword="null"/>.</returns>
        public static T? GetAttribute<T>(this IScorable component)
            where T : Attribute
            => component.Attributes.GetAttribute<T>();

        internal static T? GetAttribute<T>(this IEnumerable<Attribute> attributes)
            where T : Attribute
            => attributes.OfType<T>().FirstOrDefault();

        internal static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
            => provider.GetCustomAttributes(inherit).OfType<Attribute>();

        internal static IEnumerable<ModuleInfo> GetModules(this ComponentConfiguration configuration, IEnumerable<Type> types, ModuleInfo? parent, bool withNested)
        {
            foreach (var type in types)
            {
                if (!withNested && type.IsNested)
                    continue;

                if (!typeof(CommandModule).IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
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
                    var component = new ModuleInfo(type, parent, aliases, configuration);

                    var componentFilter = configuration.GetProperty<Func<IComponent, bool>>(ConfigurationPropertyDefinitions.ComponentRegistrationExpression);

                    if (componentFilter?.Invoke(component) ?? true)
                        yield return component;
                }
            }
        }

        internal static IEnumerable<IComponent> GetCommands(this ComponentConfiguration configuration, Type type, ModuleInfo? parent, bool withDefaults)
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
                            if (param.Length > 0 && param[0].ParameterType.IsGenericType && param[0].ParameterType.GetGenericTypeDefinition() == typeof(CommandContext<>))
                                hasContext = true;

                            component = new CommandInfo(parent, new StaticActivator(method, hasContext), aliases, hasContext, configuration);
                        }
                        else
                            component = new CommandInfo(parent, new InstanceActivator(method), aliases, false, configuration);

                        var componentFilter = configuration.GetProperty<Func<IComponent, bool>>(ConfigurationPropertyDefinitions.ComponentRegistrationExpression);

                        if (componentFilter?.Invoke(component) ?? true)
                            yield return component;
                    }
                }
            }
        }

        internal static CollectionType GetCollectionType(this Type type, Type? elementType = null)
        {
            if (elementType != null)
            {
                if (type.IsAssignableFrom(typeof(List<>).MakeGenericType(elementType)))
                    return CollectionType.List;

                if (type.IsAssignableFrom(typeof(HashSet<>).MakeGenericType(elementType)))
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
