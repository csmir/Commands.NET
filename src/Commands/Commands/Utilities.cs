using Commands.Parsing;

namespace Commands;

/// <summary>
///     Provides a set of helper functions for working with components.
/// </summary>
public static class Utilities
{
    /// <summary>
    ///     Gets an <see cref="IEnumerable{T}"/> containing all implementations of <see cref="CommandModule"/> from the provided types.
    /// </summary>
    /// <param name="types">A collection of types to create modules from.</param>
    /// <param name="options">The configuration which determines certain settings for the creation process for contained commands.</param>
    /// <param name="isNested">Determines whether the current iteration of additions is nested or not.</param>
    /// <returns>A new <see cref="IEnumerable{T}"/> containing all created component groups in the initial collection of types.</returns>
    public static IEnumerable<CommandGroup> GetComponents(this IEnumerable<Type> types, ComponentOptions options, bool isNested = false)
    {
        Assert.NotNull(types, nameof(types));
        Assert.NotNull(options, nameof(options));

        return GetComponents(options, types, isNested);
    }

    #region Internals

    #region Collections

    internal static T? FirstOrDefault<T>(this IEnumerable values)
    {
        foreach (var entry in values)
        {
            if (entry is T tEntry)
                return tEntry;
        }

        return default;
    }

    internal static void CopyTo<T>(ref T[] array, T item)
    {
        var newArray = new T[array.Length + 1];

        Array.Copy(array, newArray, array.Length);

        newArray[array.Length] = item;

        array = newArray;
    }

    internal static void CopyTo<T>(ref T[] array, T[] items)
    {
        var newArray = new T[array.Length + items.Length];

        Array.Copy(array, newArray, array.Length);

        var i = array.Length;

        foreach (var component in items)
            newArray[i++] = component;

        array = newArray;
    }

    #endregion

    #region Execution

    internal static async ValueTask<ParseResult[]> ParseParameters(IParameterCollection collection, IContext context, Arguments args, ExecutionOptions options)
    {
        var results = new ParseResult[collection.Parameters.Length];

        for (int i = 0; i < collection.Parameters.Length; i++)
        {
            var param = collection.Parameters[i];

            if (param.IsRemainder)
            {
                results[i] = await param.Parse(context, param.IsCollection ? args.TakeRemaining(param.Name!) : args.TakeRemaining(param.Name!, options.RemainderSeparator), options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);

                break;
            }

            if (param.IsResource)
            {
                results[i] = await param.Parse(context, null, options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);

                continue;
            }

            if (param is ConstructibleParameter constructible)
            {
                var result = await ParseParameters(constructible, context, args, options).ConfigureAwait(false);

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

            if (args.TryGetValue(param.Name!, out var value))
                results[i] = await param.Parse(context, value, options.ServiceProvider, options.CancellationToken).ConfigureAwait(false);
            else if (param.IsOptional)
                results[i] = ParseResult.FromSuccess(Type.Missing);
            else
                results[i] = ParseResult.FromError(new ArgumentNullException(param.Name));
        }

        return results;
    }

    internal static object?[] ResolveDependencies(DependencyParameter[] dependencies, MemberInfo target, ExecutionOptions options)
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

    #endregion

    #region Components

    internal static CommandGroup[] GetComponents(ComponentOptions configuration, IEnumerable<Type> types, bool isNested)
    {
        var output = Array.Empty<CommandGroup>();

        var action = new Action<Type>((
#if NET8_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
            type) =>
        {
            if (isNested && !type.IsNested)
                return;

            CommandGroup? group;

            try
            {
                group = new CommandGroup(type, configuration);
            }
            catch (ComponentFormatException)
            {
                // This will throw if the type does not implement CommandModule. We can safely ignore this.
                return;
            }

            if (group != null && !group.Ignore)
                CopyTo(ref output, group);
        });

        foreach (var type in types)
            action(type);

        return output;
    }

    internal static ICommandParameter[] GetParameters(IActivator activator, ComponentOptions configuration)
    {
        var parameters = activator.Target.GetParameters();

        if (activator.ContextIndex != -1)
            parameters = [.. parameters.Skip(activator.ContextIndex + 1)];

        var arr = new ICommandParameter[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].GetCustomAttributes().Any(x => x is DeconstructAttribute))
                arr[i] = new ConstructibleParameter(parameters[i], configuration);
            else
                arr[i] = new CommandParameter(parameters[i], configuration);
        }

        return arr;
    }

    internal static Tuple<int, int> GetLength(IEnumerable<ICommandParameter> arguments)
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
                // Resource parameters are not counted in the length, as they are skipped during parsing.
                if (defaultParameter.IsResource)
                    continue;

                maxLength++;
                if (!defaultParameter.IsOptional)
                    minLength++;
            }
        }

        return new(minLength, maxLength);
    }

    #endregion

    #region Reflection

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

    #endregion
}
