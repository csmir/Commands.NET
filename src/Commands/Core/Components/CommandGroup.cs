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
    public bool IsEmittedComponent
        => Type == null;

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
        CommandGroup? parent, ICondition[] conditions, string[] aliases)
        : base(false)
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
        => Activator!.Target.GetHashCode();
}
