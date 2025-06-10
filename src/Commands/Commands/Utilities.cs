using Commands.Parsing;
using Commands.Testing;

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

    /// <summary>
    ///     Tests the target command using the provided <paramref name="contextCreationDelegate"/> function to create the context for each individual execution.
    /// </summary>
    /// <remarks>
    ///     Define tests on commands using the <see cref="TestAttribute"/> attribute on the command method or delegate.
    /// </remarks>
    /// <typeparam name="TContext">The type of <see cref="IContext"/> that this test sequence should use to test with.</typeparam>
    /// <param name="command">The command to target for querying available tests, and test execution.</param>
    /// <param name="contextCreationDelegate">A delegate that yields an implementation of <typeparamref name="TContext"/> based on the input value for every new test.</param>
    /// <param name="options">A collection of options that determine how every test against this command is ran.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> containing an <see cref="IEnumerable{T}"/> with the result of every test yielded by this operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="command"/> or <paramref name="contextCreationDelegate"/> is <see langword="null"/>.</exception>
    public static async ValueTask<IEnumerable<TestResult>> Test<TContext>(this Command command, Func<string, TContext> contextCreationDelegate, ExecutionOptions? options = null)
        where TContext : class, IContext
    {
        static TestResult Compare(ITest test, TestResultType targetType, Exception exception)
        {
            return test.ShouldEvaluateTo == targetType
                ? TestResult.FromSuccess(test, test.ShouldEvaluateTo)
                : TestResult.FromError(test, test.ShouldEvaluateTo, targetType, exception);
        }

        Assert.NotNull(command, nameof(command));
        Assert.NotNull(contextCreationDelegate, nameof(contextCreationDelegate));

        options ??= ExecutionOptions.Default;

        if (options.ExecuteAsynchronously)
            options.ExecuteAsynchronously = false;

        var tests = command.Attributes.OfType<ITest>().ToArray();

        var results = new TestResult[tests.Length];

        for (var i = 0; i < tests.Length; i++)
        {
            var test = tests[i];

            var fullName = string.IsNullOrWhiteSpace(test.Arguments)
                ? command.GetFullName(false)
                : command.GetFullName(false) + ' ' + test.Arguments;

            var runResult = await command.Run(contextCreationDelegate(fullName), options).ConfigureAwait(false);

            results[i] = runResult.Exception switch
            {
                null => Compare(test, TestResultType.Success, new InvalidOperationException("The command was expected to fail, but it succeeded.")),

                CommandParsingException => Compare(test, TestResultType.ParseFailure, runResult.Exception),
                CommandEvaluationException => Compare(test, TestResultType.ConditionFailure, runResult.Exception),
                CommandOutOfRangeException => Compare(test, TestResultType.MatchFailure, runResult.Exception),

                _ => Compare(test, TestResultType.InvocationFailure, runResult.Exception),
            };
        }

        return results;
    }

    #region Internals

    internal static async ValueTask<ParseResult[]> Parse(this IParameterCollection collection, IContext context, Arguments args, ExecutionOptions options)
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
                var result = await Parse(constructible, context, args, options).ConfigureAwait(false);

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

    internal static IEnumerable<IComponent> GetNestedComponents(this CommandGroup parent, ComponentOptions configuration)
    {
        static IEnumerable<IComponent> GetExecutableComponents(CommandGroup parent, ComponentOptions configuration)
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

        var commands = GetExecutableComponents(parent, configuration);

        try
        {
            var nestedTypes = parent.Activator.Type.GetNestedTypes(BindingFlags.Public);

            var groups = GetComponents(configuration, nestedTypes, true);

            return commands.Concat(groups);
        }
        catch
        {
            // Do nothing, we can't access nested types.
            return commands;
        }
    }

    internal static ICommandParameter[] GetParameters(this IActivator activator, ComponentOptions configuration)
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
                if (defaultParameter.IsResource)
                    continue;

                maxLength++;
                if (!defaultParameter.IsOptional)
                    minLength++;
            }

            // Resource parameters are not counted in the length, as they are skipped during parsing.
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

    private static CommandGroup[] GetComponents(ComponentOptions configuration, IEnumerable<Type> types, bool isNested)
    {
        Assert.NotNull(types, nameof(types));

        var output = Array.Empty<CommandGroup>();

        types.ForEach((
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
                Collection.CopyTo(ref output, group);
        });

        return output;
    }

    #endregion
}
