using Commands.Conversion;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        ///     Browses through the types known in the <paramref name="types"/> and returns every discovered top-level module.
        /// </summary>
        /// <param name="configuration">The configuration that defines the command registration process.</param>
        /// <param name="types">The types that should be searched to discover new modules.</param>
        /// <returns>A lazily evaluated <see cref="IEnumerable{T}"/> containing all discovered modules in the provided <paramref name="types"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<ModuleInfo> GetComponents(this ComponentConfiguration configuration, params TypeDefinition[] types)
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
#if NET8_0_OR_GREATER
        [UnconditionalSuppressMessage("AotAnalysis", "IL2062", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
        public static IEnumerable<IComponent> GetNestedComponents(this ComponentConfiguration configuration, ModuleInfo parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (parent.Type == null)
                return [];

            var commands = configuration.GetCommands(parent, parent.Aliases.Length > 0);

            try
            {
                var nestedTypes = parent.Type.GetNestedTypes(BindingFlags.Public);

                var modules = configuration.GetModules([.. nestedTypes], parent, true);

                return commands.Concat(modules);
            }
            catch
            {
                return commands;
                // Do nothing, we simply cannot get the nested types.
            }
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

                    throw BuildException.ParserNotSupported(elementType);
                }

                return parser;
            }

            
            if (configuration.Parsers.TryGetValue(type, out var parser))
                return parser;

            if (type.IsEnum)
                return EnumParser.GetOrCreate(type);

            if (type.IsArray)
                return ArrayParser.GetOrCreate(GetParser(type.GetElementType()!));

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

        internal static IEnumerable<ModuleInfo> GetModules(this ComponentConfiguration configuration, TypeDefinition[] types, ModuleInfo? parent, bool withNested)
        {
            foreach (var definition in types)
            {
                var type = definition.Value;

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

                    if (attribute is IgnoreAttribute doSkip)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip)
                {
                    // yield a new module if all aliases are valid and it shouldn't be skipped.
                    var component = new ModuleInfo(type, parent, aliases, configuration);

                    var componentFilter = configuration.GetProperty<Func<IComponent, bool>>(ConfigurationPropertyDefinitions.ComponentRegistrationFilterExpression);

                    if (componentFilter?.Invoke(component) ?? true)
                        yield return component;
                }
            }
        }

        internal static IEnumerable<IComponent> GetCommands(this ComponentConfiguration configuration, ModuleInfo parent, bool withDefaults)
        {
            var members = parent.Type!.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            foreach (var method in members)
            {
                var aliases = Array.Empty<string>();

                var skip = false;
                foreach (var attribute in method.GetCustomAttributes(true))
                {
                    if (attribute is NameAttribute names)
                    {
                        names.ValidateAliases(configuration);

                        aliases = names.Aliases;
                        continue;
                    }

                    if (attribute is IgnoreAttribute doSkip)
                    {
                        skip = true;
                        break;
                    }
                }

                if (!skip && (withDefaults || aliases.Length > 0))
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

                    var componentFilter = configuration.GetProperty<Func<IComponent, bool>>(ConfigurationPropertyDefinitions.ComponentRegistrationFilterExpression);

                    if (componentFilter?.Invoke(component) ?? true)
                        yield return component;
                }
            }
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

        internal static ConstructorInfo GetInvokableConstructor(
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            this Type type)
        {
            var ctors = type.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length);

            foreach (var ctor in ctors)
            {
                if (ctor.GetCustomAttributes().Any(attr => attr is IgnoreAttribute))
                    continue;

                return ctor;
            }

            throw new InvalidOperationException($"{type} has no public constructors that are accessible for this type to be constructed.");
        }
    }
}
