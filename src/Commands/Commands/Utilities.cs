using Commands.Conditions;
using Commands.Parsing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    public static IEnumerable<CommandGroup> GetComponents(this IEnumerable<Type> types, ComponentOptions? options = null, bool isNested = false)
        => GetComponents(options ?? ComponentOptions.Default, types, isNested);

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

#if NET6_0_OR_GREATER

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CopyTo<T>(ref T[] array, T item)
    {
        T[] newArray = GC.AllocateUninitializedArray<T>(array.Length + 1);

        ref T source = ref MemoryMarshal.GetArrayDataReference(array), destination = ref MemoryMarshal.GetArrayDataReference(newArray);

        for (int i = 0; i < array.Length; i++) Unsafe.Add(ref destination, i) = Unsafe.Add(ref source, i);

        Unsafe.Add(ref destination, array.Length) = item;

        array = newArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CopyTo<T>(ref T[] array, T[] items)
    {
        T[] newArray = GC.AllocateUninitializedArray<T>(array.Length + items.Length);

        ref T sourceA = ref MemoryMarshal.GetArrayDataReference(array), sourceB = ref MemoryMarshal.GetArrayDataReference(items), destination = ref MemoryMarshal.GetArrayDataReference(newArray);

        for (int i = 0; i < array.Length; i++) Unsafe.Add(ref destination, i) = Unsafe.Add(ref sourceA, i);

        for (int i = 0; i < items.Length; i++) Unsafe.Add(ref destination, array.Length + i) = Unsafe.Add(ref sourceB, i);

        array = newArray;
    }

#else

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

#endif

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

    internal static void ResolveDependencies(ref object?[] args, DependencyParameter[] dependencies, MemberInfo target, ExecutionOptions options)
    {
        if (dependencies.Length == 0)
            return;

        var resolver = (options.ServiceProvider.GetService(typeof(IDependencyResolver))
            ?? new DefaultDependencyResolver(options.ServiceProvider)) as IDependencyResolver;

        for (var i = 0; i < dependencies.Length; i++)
        {
            var dependency = dependencies[i];

            var service = resolver!.GetService(dependency);

            if (service != null || dependency.IsNullable)
                args[dependency.Position] = service;

            else if (dependency.Type == typeof(IServiceProvider))
                args[dependency.Position] = options.ServiceProvider;

            else if (dependency.Type == typeof(IComponentProvider))
                args[dependency.Position] = options.ComponentProvider;

            else if (dependency.IsOptional)
                args[dependency.Position] = Type.Missing;

            else
                throw new ComponentFormatException($"The method or module {target.Name} defines unknown service type {dependency.Type}.");
        }
    }

    #endregion

    #region Components

    internal static CommandGroup[] GetComponents(ComponentOptions configuration, IEnumerable<Type> types, bool isNested)
    {
        var output = Array.Empty<CommandGroup>();

        var action = new Action<Type>((
#if NET6_0_OR_GREATER
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

    internal static ICommandParameter[] GetCommandParameters(ParameterInfo[] parameters, DependencyParameter[] dependencies, int contextIndex, ComponentOptions options)
    {
        // The parameters that are dependencies are not included in the command parameters.
        var realIndex = 0;
        var realLength = parameters.Length - dependencies.Length;

        if (contextIndex != -1)
            realLength--;

        var output = new ICommandParameter[realLength];

        for (var i = 0; i < parameters.Length; i++)
        {
            // If a dependency has already been defined at this position, skip it.
            if (dependencies.Any(d => d.Position == i) || i == contextIndex)
                continue;

            if (parameters[i].GetCustomAttributes().Any(x => x is DeconstructAttribute))
                output[realIndex++] = new ConstructibleParameter(parameters[i], options);
            else
                output[realIndex++] = new CommandParameter(parameters[i], options);
        }

        return output;
    }

    internal static Tuple<int, int> GetLength(IEnumerable<ICommandParameter> parameters)
    {
        var minLength = 0;
        var maxLength = 0;

        foreach (var parameter in parameters)
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

    internal static IEvaluator[] GetEvaluators(IEnumerable<ExecuteConditionAttribute> conditions)
    {
        static IEnumerable<IEvaluator> YieldEvaluators(IEnumerable<IGrouping<string, ExecuteConditionAttribute>> groups)
        {
            foreach (var group in groups)
            {
                var groupArr = group.ToArray();

                var evaluator = groupArr[0].CreateEvaluator();

                if (evaluator.MaximumAllowedConditions.HasValue && groupArr.Length > evaluator.MaximumAllowedConditions.Value)
                    throw new ComponentFormatException($"The evaluator {evaluator.GetType()} specifies that only {evaluator.MaximumAllowedConditions.Value} conditions of its scope are permitted per signature, but it discovered {groupArr.Length} conditions.");

                evaluator.Conditions = groupArr;

                yield return evaluator;
            }
        }

        if (!conditions.Any())
            return [];

        var evaluatorGroups = conditions
            .GroupBy(x => x.EvaluatorName);

        return [.. YieldEvaluators(evaluatorGroups).OrderBy(x => x.Order)];
    }

    #endregion

    #region Reflection

    internal static ConstructorInfo GetAvailableConstructor(
#if NET6_0_OR_GREATER
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
