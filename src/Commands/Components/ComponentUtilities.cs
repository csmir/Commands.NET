using System.Reflection;

namespace Commands;

internal static class ComponentUtilities
{
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

    internal static IEnumerable<CommandGroup> BuildGroups(this ComponentConfiguration configuration, DynamicType[] types, CommandGroup? parent, bool isNested)
    {
        Assert.NotNull(types, nameof(types));

        foreach (var definition in types)
        {
            var type = definition.Value;

            if (!isNested && type.IsNested)
                continue;

            if (!typeof(CommandModule).IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
                continue;

            var names = Array.Empty<string>();

            var ignore = false;
            foreach (var attribute in type.GetCustomAttributes(true))
            {
                if (attribute is NameAttribute nameAttr)
                {
                    // Validate names. Nested groups are invalid if they have no names, so we invert the nested flag to not permit this.
                    Assert.Names(nameAttr.Names, configuration, !isNested);

                    names = nameAttr.Names;

                    continue;
                }

                if (attribute is IgnoreAttribute)
                {
                    ignore = true;
                    break;
                }
            }

            // Nested groups are invalid if they have no names.
            if (!ignore && (!isNested || names.Length > 0))
                yield return new CommandGroup(type, parent, names, configuration);
        }
    }

    internal static IEnumerable<IComponent> BuildCommands(this ComponentConfiguration configuration, CommandGroup parent, bool isNested)
    {
        var members = parent.Type!.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

        foreach (var method in members)
        {
            var names = Array.Empty<string>();

            var ignore = false;
            foreach (var attribute in method.GetCustomAttributes(true))
            {
                if (attribute is NameAttribute nameAttr)
                {
                    // Validate names. Nested commands are valid if they have no names.
                    Assert.Names(nameAttr.Names, configuration, isNested);

                    names = nameAttr.Names;
                    continue;
                }

                if (attribute is IgnoreAttribute)
                {
                    ignore = true;
                    break;
                }
            }

            // Nested commands are valid if they have no names.
            if (!ignore && (isNested || names.Length > 0))
            {
                Command? component;
                if (method.IsStatic)
                {
                    var hasContext = method.HasContext();

                    component = new Command(parent, new CommandStaticActivator(method, hasContext), names, hasContext, configuration);
                }
                else
                    component = new Command(parent, new CommandInstanceActivator(method), names, configuration);

                yield return component;
            }
        }
    }

#if NET8_0_OR_GREATER
    [UnconditionalSuppressMessage("AotAnalysis", "IL2062", Justification = "The type is propagated from user-facing code, it is up to the user to make it available at compile-time.")]
#endif
    internal static IEnumerable<IComponent> BuildNestedComponents(this ComponentConfiguration configuration, CommandGroup parent)
    {
        Assert.NotNull(parent, nameof(parent));

        if (parent.Type == null)
            return [];

        var commands = configuration.BuildCommands(parent, parent.Names.Length > 0);

        try
        {
            var nestedTypes = parent.Type.GetNestedTypes(BindingFlags.Public);

            var groups = configuration.BuildGroups([.. nestedTypes], parent, true);

            return commands.Concat(groups);
        }
        catch
        {
            return commands;
            // Do nothing, we simply cannot get the nested types.
        }
    }

    internal static ICommandParameter[] BuildArguments(this MethodBase method, bool withContext, ComponentConfiguration configuration)
    {
        var parameters = method.GetParameters();

        if (withContext)
            parameters = parameters.Skip(1).ToArray();

        var arr = new ICommandParameter[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var complex = false;
            var name = string.Empty;
            foreach (var attr in parameters[i].GetCustomAttributes())
            {
                if (attr is DeconstructAttribute)
                    complex = true;

                if (attr is NameAttribute names)
                {
                    // names is not supported for parameters.
                    name = names.Name;
                }
            }

            if (complex)
                arr[i] = new ConstructibleParameter(parameters[i], name, configuration);
            else
                arr[i] = new CommandParameter(parameters[i], name, configuration);
        }

        return arr;
    }

    internal static bool HasContext(this MethodBase method)
        => method.GetParameters().Length > 0 && method.GetParameters()[0].ParameterType.IsGenericType && method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(CommandContext<>);

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

    internal static Tuple<int, int> GetLength(this IEnumerable<ICommandParameter> arguments)
    {
        var minLength = 0;
        var maxLength = 0;

        foreach (var parameter in arguments)
        {
            if (parameter is ConstructibleParameter complexArgument)
            {
                maxLength += complexArgument.MaxLength;
                minLength += complexArgument.MinLength;
            }

            if (parameter is CommandParameter defaultArgument)
            {
                maxLength++;
                if (!defaultArgument.IsOptional)
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

    internal static async ValueTask<ParseResult[]> Parse(this Command command, ICallerContext caller, int parseIndex, ArgumentArray args, CommandOptions options)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        args.SetParseIndex(parseIndex);

        if (!command.HasParameters && args.Length == 0)
            return [];

        if (command.MaxLength == args.Length)
            return await command.Parse(caller, args, options);

        if (command.MaxLength <= args.Length && command.HasRemainder)
            return await command.Parse(caller, args, options);

        if (command.MaxLength > args.Length && command.MinLength <= args.Length)
            return await command.Parse(caller, args, options);

        return [ParseResult.FromError(ParseException.ArgumentMismatch(command.MinLength, args.Length))];
    }

    internal static async ValueTask<ParseResult[]> Parse(this IParameterCollection provider, ICallerContext caller, ArgumentArray args, CommandOptions options)
    {
        options.CancellationToken.ThrowIfCancellationRequested();

        var results = new ParseResult[provider.Parameters.Length];

        for (int i = 0; i < provider.Parameters.Length; i++)
        {
            var argument = provider.Parameters[i];

            if (argument.IsRemainder)
            {
                results[i] = await argument.Parse(caller, argument.IsCollection ? args.TakeRemaining(argument.Name!) : args.TakeRemaining(argument.Name!, options.RemainderSeparator), options.Services, options.CancellationToken);

                break;
            }

            if (argument is ConstructibleParameter complexParameter)
            {
                var result = await complexParameter.Parse(caller, args, options);

                if (result.All(x => x.Success))
                {
                    try
                    {
                        results[i] = ParseResult.FromSuccess(complexParameter.Activator.Invoke(caller, null, result.Select(x => x.Value).ToArray(), options));
                    }
                    catch (Exception ex)
                    {
                        results[i] = ParseResult.FromError(ex);
                    }

                    continue;
                }

                if (complexParameter.IsOptional)
                    results[i] = ParseResult.FromSuccess(Type.Missing);

                continue;
            }

            if (args.TryGetElement(argument.Name!, out var value))
                results[i] = await argument.Parse(caller, value, options.Services, options.CancellationToken);
            else if (argument.IsOptional)
                results[i] = ParseResult.FromSuccess(Type.Missing);
            else
                results[i] = ParseResult.FromError(new ArgumentNullException(argument.Name));
        }

        return results;
    }
}
