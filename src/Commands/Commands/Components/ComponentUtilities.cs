using Commands.Parsing;
using System.ComponentModel;

namespace Commands;

/// <summary>
///     Provides a set of helper functions for working with components.
/// </summary>
public static class ComponentUtilities
{
    /// <summary>
    ///     Gets the first entry of the specified type, or <see langword="null"/> if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type to filter.</typeparam>
    /// <param name="values"></param>
    /// <returns>The first occurrence of <typeparamref name="T"/> in the collection if any exists, otherwise <see langword="null"/>.</returns>
    public static T? FirstOrDefault<T>(this IEnumerable values)
    {
        foreach (var entry in values)
        {
            if (entry is T tEntry)
                return tEntry;
        }

        return default;
    }

    /// <summary>
    ///     Checks if the collection contains an instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to filter.</typeparam>
    /// <param name="values"></param>
    /// <returns><see langword="true"/> if a any <typeparamref name="T"/> was found, otherwise <see langword="false"/>.</returns>
    public static bool Contains<T>(this IEnumerable values)
    {
        foreach (var entry in values)
        {
            if (entry is T)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Gets all instances of the specified type, matching the provided predicate.
    /// </summary>
    /// <typeparam name="T">The type to filter.</typeparam>
    /// <param name="values"></param>
    /// <param name="predicate">The predicate which determines whether the component can be returned or not.</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all legible values of <typeparamref name="T"/> in the initial collection.</returns>
    public static IEnumerable<T> OfType<T>(this IEnumerable values, Predicate<T> predicate)
    {
        foreach (var entry in values)
        {
            if (entry is T tEntry && predicate(tEntry))
                yield return tEntry;
        }
    }

    /// <summary>
    ///     Gets an <see cref="IEnumerable{T}"/> containing all implementations of <see cref="CommandModule"/> from the provided types.
    /// </summary>
    /// <param name="types">A collection of types to create modules from.</param>
    /// <param name="options">The configuration which determines certain settings for the creation process for contained commands.</param>
    /// <param name="parent">The parent of this collection of types, if any.</param>
    /// <param name="isNested">Determines whether the current iteration of additions is nested or not.</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all created component groups in the initial collection of types.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2067", Justification = "The types are supplied from user-facing implementation, it is up to the user to ensure that these types are available in AOT context.")]
#endif
    public static IEnumerable<CommandGroup> GetComponents(IEnumerable<Type> types, ComponentOptions options, CommandGroup? parent = null, bool isNested = false)
    {
        Assert.NotNull(types, nameof(types));
        Assert.NotNull(options, nameof(options));

        return GetComponents(options, types.Select(x => new TypeWrapper(x)), parent, isNested);
    }

    #region Internals

    internal static async ValueTask<ParseResult[]> Parse(IParameterCollection collection, IContext context, ExecutionOptions options)
    {
        var results = new ParseResult[collection.Parameters.Length];

        for (int i = 0; i < collection.Parameters.Length; i++)
        {
            var param = collection.Parameters[i];

            if (param.IsRemainder)
            {
                results[i] = await param.Parse(context, param.IsCollection ? context.Arguments.TakeRemaining(param.Name!) : context.Arguments.TakeRemaining(param.Name!, options.RemainderSeparator), options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);

                break;
            }

            if (param is ConstructibleParameter constructible)
            {
                var result = await Parse(constructible, context, options).ConfigureAwait(false);

                if (result.All(x => x.Success))
                {
                    try
                    {
                        results[i] = ParseResult.FromSuccess(constructible.Activator.Invoke(context, null, [.. result.Select(x => x.Value)], options));
                    }
                    catch (Exception ex)
                    {
                        results[i] = ParseResult.FromError(ex);
                    }

                    continue;
                }

                if (constructible.IsOptional)
                    results[i] = ParseResult.FromSuccess(Type.Missing);

                continue;
            }

            if (context.Arguments.TryGetValue(param.Name!, out var value))
                results[i] = await param.Parse(context, value, options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);
            else if (param.IsOptional)
                results[i] = ParseResult.FromSuccess(Type.Missing);
            else
                results[i] = ParseResult.FromError(new ArgumentNullException(param.Name));
        }

        return results;
    }

    internal static object?[] Resolve(this DependencyParameter[] dependencies, MemberInfo target, ExecutionOptions options)
    {
        if (dependencies.Length == 0)
            return [];

        var resolver = (options.ServiceProvider.GetService(typeof(IDependencyResolver))
            ?? new DefaultDependencyResolver(options.ServiceProvider)) as IDependencyResolver;

        var resolvedValues = new object?[dependencies.Length];

        for (var i = 0; i < dependencies.Length; i++)
        {
            var dependency = dependencies[i];

            var service = resolver!.GetService(dependency);

            if (service != null || dependency.IsNullable)
                resolvedValues[i] = service;

            else if (dependency.Type == typeof(IServiceProvider))
                resolvedValues[i] = options.ServiceProvider;

            else if (dependency.Type == typeof(IComponentProvider))
                resolvedValues[i] = options.ComponentProvider;

            else if (dependency.IsOptional)
                resolvedValues[i] = Type.Missing;

            else
                throw new ComponentFormatException($"The method or module {target.Name} defines unknown service type {dependency.Type}.");
        }

        return resolvedValues;
    }

    internal static IEnumerable<CommandGroup> GetComponents(ComponentOptions configuration, IEnumerable<TypeWrapper> types, CommandGroup? parent, bool isNested)
    {
        Assert.NotNull(types, nameof(types));

        foreach (var definition in types)
        {
            var type = definition.Value;

            if (!isNested && type.IsNested)
                continue;

            CommandGroup? group;

            try
            {
                group = new CommandGroup(type, parent, configuration);
            }
            catch (ComponentFormatException)
            {
                // This will throw if the type does not implement CommandModule. We can safely ignore this.
                continue;
            }

            if (group != null && !group.Ignore)
                yield return group;
        }
    }

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2062", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
    internal static IEnumerable<IComponent> GetNestedComponents(ComponentOptions configuration, CommandGroup parent)
    {
        static IEnumerable<IComponent> GetExecutableComponents(ComponentOptions configuration, CommandGroup parent)
        {
            var members = parent.Activator!.Type!.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

            foreach (var method in members)
            {
                var command = new Command(method, parent, configuration);

                if (command.Ignore)
                    continue;

                yield return command;
            }
        }

        Assert.NotNull(parent, nameof(parent));

        if (parent.Activator!.Type == null)
            return [];

        var commands = GetExecutableComponents(configuration, parent);

        try
        {
            var nestedTypes = parent.Activator.Type.GetNestedTypes(BindingFlags.Public);

            var groups = GetComponents(configuration, [.. nestedTypes], parent, true);

            return commands.Concat(groups);
        }
        catch
        {
            // Do nothing, we can't access nested types.
            return commands;
        }
    }

    internal static ICommandParameter[] GetParameters(IActivator activator, ComponentOptions configuration)
    {
        var parameters = activator.Target.GetParameters();

        if (activator.ContextIndex != -1)
            parameters = [.. parameters.Skip(activator.ContextIndex + 1)];

        var arr = new ICommandParameter[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].GetCustomAttributes().Contains<DeconstructAttribute>())
                arr[i] = new ConstructibleParameter(parameters[i], configuration);
            else
                arr[i] = new CommandParameter(parameters[i], configuration);
        }

        return arr;
    }

    internal static Tuple<int, int> GetLength(this IEnumerable<ICommandParameter> arguments)
    {
        var minLength = 0;
        var maxLength = 0;

        foreach (var parameter in arguments)
        {
            if (parameter is ConstructibleParameter constructibleParameter)
            {
                maxLength += constructibleParameter.MaxLength;
                minLength += constructibleParameter.MinLength;
            }

            if (parameter is CommandParameter defaultParameter)
            {
                maxLength++;
                if (!defaultParameter.IsOptional)
                    minLength++;
            }
        }

        return new(minLength, maxLength);
    }

    internal static ConstructorInfo GetAvailableConstructor(
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

        throw new ComponentFormatException($"{type} has no publically available constructors to use in creating instances of this type.");
    }

    internal static IEnumerable<Attribute> GetAttributes(this ICustomAttributeProvider provider, bool inherit)
        => provider.GetCustomAttributes(inherit).OfType<Attribute>();

    #endregion
}
