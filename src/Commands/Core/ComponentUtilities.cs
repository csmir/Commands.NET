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
        public static IEnumerable<ModuleInfo> BuildComponents(this ComponentConfiguration configuration, params TypeDefinition[] types)
        {
            Assert.NotNull(types, nameof(types));

            return configuration.BuildModules(types, null, false);
        }

        /// <summary>
        ///     Gets the first attribute of the specified type set on this component, if it exists.
        /// </summary>
        /// <typeparam name="T">The attribute type to filter by.</typeparam>
        /// <param name="component">The component that should be searched for the attribute.</param>
        /// <returns>An attribute of the type <typeparamref name="T"/> if it exists; Otherwise <see langword="null"/>.</returns>
        public static T? GetAttribute<T>(this IScorable component)
            where T : Attribute
            => component.Attributes.FirstOrDefault<T>();

        internal static T? FirstOrDefault<T>(this IEnumerable<Attribute> attributes)
            where T : Attribute
            => attributes.OfType<T>().FirstOrDefault();

        internal static bool Contains<T>(this IEnumerable<Attribute> attributes, bool allowMultipleMatches)
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

        internal static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
            => provider.GetCustomAttributes(inherit).OfType<Attribute>();

        internal static IEnumerable<ModuleInfo> BuildModules(this ComponentConfiguration configuration, TypeDefinition[] types, ModuleInfo? parent, bool isNested)
        {
            foreach (var definition in types)
            {
                var type = definition.Value;

                if (!isNested && type.IsNested)
                    continue;

                if (!typeof(CommandModule).IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
                    continue;

                var aliases = Array.Empty<string>();

                var ignore = false;
                foreach (var attribute in type.GetCustomAttributes(true))
                {
                    if (attribute is NameAttribute names)
                    {
                        // Validate aliases. Nested modules are invalid if they have no aliases, so we invert the nested flag to not permit this.
                        Assert.Aliases(names.Aliases, configuration, !isNested);

                        aliases = names.Aliases;

                        continue;
                    }

                    if (attribute is IgnoreAttribute shouldIgnore)
                    {
                        ignore = true;
                        break;
                    }
                }

                // Nested modules are invalid if they have no aliases.
                if (!ignore && (!isNested || aliases.Length > 0))
                {
                    var component = new ModuleInfo(type, parent, aliases, configuration);

                    var componentFilter = configuration.GetProperty<Func<IComponent, bool>>(ConfigurationPropertyDefinitions.ComponentRegistrationFilterExpression);

                    if (componentFilter?.Invoke(component) ?? true)
                        yield return component;
                }
            }
        }

        internal static IEnumerable<IComponent> BuildCommands(this ComponentConfiguration configuration, ModuleInfo parent, bool isNested)
        {
            var members = parent.Type!.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            foreach (var method in members)
            {
                var aliases = Array.Empty<string>();

                var ignore = false;
                foreach (var attribute in method.GetCustomAttributes(true))
                {
                    if (attribute is NameAttribute names)
                    {
                        // Validate aliases. Nested commands are valid if they have no aliases.
                        Assert.Aliases(names.Aliases, configuration, isNested);

                        aliases = names.Aliases;
                        continue;
                    }

                    if (attribute is IgnoreAttribute shouldIgnore)
                    {
                        ignore = true;
                        break;
                    }
                }

                // Nested commands are valid if they have no aliases.
                if (!ignore && (isNested || aliases.Length > 0))
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
                        component = new CommandInfo(parent, new InstanceActivator(method), aliases, configuration);

                    var componentFilter = configuration.GetProperty<Func<IComponent, bool>>(ConfigurationPropertyDefinitions.ComponentRegistrationFilterExpression);

                    if (componentFilter?.Invoke(component) ?? true)
                        yield return component;
                }
            }
        }

#if NET8_0_OR_GREATER
        [UnconditionalSuppressMessage("AotAnalysis", "IL2062", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
        internal static IEnumerable<IComponent> BuildNestedComponents(this ComponentConfiguration configuration, ModuleInfo parent)
        {
            Assert.NotNull(parent, nameof(parent));

            if (parent.Type == null)
                return [];

            var commands = configuration.BuildCommands(parent, parent.Aliases.Length > 0);

            try
            {
                var nestedTypes = parent.Type.GetNestedTypes(BindingFlags.Public);

                var modules = configuration.BuildModules([.. nestedTypes], parent, true);

                return commands.Concat(modules);
            }
            catch
            {
                return commands;
                // Do nothing, we simply cannot get the nested types.
            }
        }

        internal static IArgument[] BuildArguments(this MethodBase method, bool withContext, ComponentConfiguration configuration)
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

        internal static TypeParser GetParser(this ComponentConfiguration configuration, Type type)
        {
            Assert.NotNull(type, nameof(type));

            if (configuration.Parsers.TryGetValue(type, out var parser))
                return parser;

            if (type.IsEnum)
                return EnumParser.GetOrCreate(type);

            if (type.IsArray)
            {
                type = type.GetElementType()!;

                if (configuration.Parsers.TryGetValue(type, out parser))
                    return parser;

                if (type.IsEnum)
                    return EnumParser.GetOrCreate(type);
            }

            throw new NotSupportedException($"No parser is known for type {type}.");
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

        internal static IEnumerable<ConstructorInfo> GetAvailableConstructors(
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
#endif
            this Type type, bool allowMultipleMatches = false)
        {
            var ctors = type.GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length);

            var found = false;
            foreach (var ctor in ctors)
            {
                if (ctor.GetCustomAttributes().Any(attr => attr is IgnoreAttribute))
                    continue;

                if (!allowMultipleMatches)
                {
                    if (!found)
                    {
                        found = true;
                        yield return ctor;
                    }
                    else
                        yield break;
                }

                yield return ctor;
            }

            if (!found)
                throw new InvalidOperationException($"{type} has no publically available constructors to use in creating instances of this type.");
        }
    }
}
