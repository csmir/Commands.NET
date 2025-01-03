using Commands.Conditions;

namespace Commands;

/// <summary>
///     Reveals information about a command module, hosting zero-or-more commands.
/// </summary>
[DebuggerDisplay("Count = {Count}, {ToString()}")]
public sealed class CommandGroup : ComponentCollection, IComponent
{
    /// <summary>
    ///     Gets the type of this module.
    /// </summary>
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public Type? Type { get; }

    /// <summary>
    ///     Gets the depth of the module, being how deeply nested it is in the command tree.
    /// </summary>
    public int Depth
        => Parent?.Depth + 1 ?? 1;

    /// <inheritdoc />
    public IActivator? Activator { get; }

    /// <inheritdoc />
    public string[] Aliases { get; }

    /// <inheritdoc />
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public ConditionEvaluator[] Evaluators { get; }

    /// <inheritdoc />
    public CommandGroup? Parent { get; }

    /// <inheritdoc />
    public string? Name
        => Aliases.Length > 0 ? Aliases[0] : null;

    /// <inheritdoc />
    public string FullName
        => $"{(Parent != null && Parent.Name != null ? $"{Parent.FullName} " : "")}{Name}";

    /// <inheritdoc />
    public float Score
        => GetScore();

    /// <inheritdoc />
    public bool IsSearchable
        => Aliases.Length > 0;

    /// <inheritdoc />
    public bool IsDefault
        => false;

    internal CommandGroup(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type, CommandGroup? parent, string[] aliases, ComponentConfiguration configuration)
        : base(configuration.GetProperty("MakeModulesReadonly", false))
    {
        Parent = parent;
        Type = type;

        var attributes = type.GetAttributes(true);

        Attributes = attributes.ToArray();
        Evaluators = ConditionEvaluator.CreateEvaluators(attributes.OfType<ICondition>()).Concat(parent?.Evaluators ?? []).ToArray();

        Aliases = aliases;

        Activator = new CommandGroupActivator(type);

        Push(configuration.BuildNestedComponents(this).OrderByDescending(x => x.Score));
    }

    internal CommandGroup(
        CommandGroup? parent, ICondition[] conditions, string[] aliases, ComponentConfiguration configuration)
        : base(configuration.GetProperty("MakeModulesReadonly", false))
    {
        Parent = parent;

        Attributes = [];
        Evaluators = ConditionEvaluator.CreateEvaluators(conditions).Concat(parent?.Evaluators ?? []).ToArray();

        Aliases = aliases;
    }

    /// <inheritdoc />
    public float GetScore()
    {
        if (Count == 0)
            return 0.0f;

        var score = 1.0f;

        foreach (var component in this)
            score += component.GetScore();

        if (Name != Type?.Name)
            score += 1.0f;

        score += Attributes.FirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

        return score;
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
        => obj is IScorable scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public bool Equals(IComponent? other)
        => other is CommandGroup info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override IEnumerable<SearchResult> Find(ArgumentEnumerator args)
    {
        List<SearchResult> discovered =
        [
            SearchResult.FromError(this)
        ];

        var searchHeight = Depth;

        foreach (var component in this)
        {
            if (component.IsDefault)
                discovered.Add(SearchResult.FromSuccess(component, searchHeight));

            if (args.TryNext(searchHeight, out var value) && component.Aliases.Contains(value))
            {
                if (component is CommandGroup module)
                    discovered.AddRange(module.Find(args));
                else
                    discovered.Add(SearchResult.FromSuccess(component, searchHeight + 1));
            }
        }

        return discovered;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"{(Parent != null ? $"{Parent}." : "")}{(Name != null ? $"{Type?.Name}['{Name}']" : $"{Type?.Name}")}";

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is CommandGroup info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    /// <summary>
    ///    Creates a new empty command group from the provided aliases, conditions and configuration.
    /// </summary>
    /// <param name="aliases">A set of names by which the command group will be able to be discovered.</param>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <returns>A new <b>empty</b> instance of <see cref="CommandGroup"/>, able to be mutated using exposed API's.</returns>
    public static CommandGroup Create(string[] aliases, ComponentConfiguration? configuration = null)
    {
        configuration ??= ComponentConfiguration.Default;

        Assert.NotNull(aliases, nameof(aliases));
        Assert.Aliases(aliases, configuration, false);

        return new CommandGroup(null, [], aliases, configuration);
    }

    /// <inheritdoc cref="Create(Type, ComponentConfiguration?)"/>
    /// <typeparam name="T">A type implementing <see cref="CommandModule"/> or <see cref="CommandModule{T}"/>.</typeparam>
    public static CommandGroup Create<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    T>(ComponentConfiguration? configuration = null)
        where T : CommandModule
        => Create(typeof(T), configuration);

    /// <summary>
    ///     Creates a new command group from the provided type and configuration.
    /// </summary>
    /// <remarks>
    ///     Creating a command group through this method will expect a name is provided to the group.
    /// </remarks>
    /// <param name="type">A type implementing <see cref="CommandModule"/> or <see cref="CommandModule{T}"/>.</param>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <returns>A new instance of <see cref="CommandGroup"/> containing all discovered commands, subgroups and subcommands within the provided type.</returns>
    public static CommandGroup Create(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type, ComponentConfiguration? configuration = null)
    {
        configuration ??= ComponentConfiguration.Default;

        Assert.NotNull(type, nameof(type));

        if (!typeof(CommandModule).IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
            throw new ArgumentException("The type must be a non-abstract, non-generic, and assignable to CommandModule to be considered a valid implementation.", nameof(type));

        var aliases = type.GetAttributes(false).FirstOrDefault<NameAttribute>()?.Aliases ?? [];

        Assert.Aliases(aliases, configuration, false);

        return new CommandGroup(type, null, aliases, configuration ?? new ComponentConfiguration());
    }
}
