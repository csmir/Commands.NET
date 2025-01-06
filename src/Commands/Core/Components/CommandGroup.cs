using Commands.Builders;
using Commands.Conditions;
using System.Text;

namespace Commands;

/// <summary>
///     Reveals information about a command module, hosting zero-or-more commands.
/// </summary>
[DebuggerDisplay("Count = {Count}, {ToString()}")]
public sealed class CommandGroup : ComponentCollection, IComponent, ICommandSegment
{
    /// <inheritdoc />
    public CommandGroup? Parent { get; private set; }

    /// <inheritdoc />
    public IActivator? Activator { get; }

    /// <inheritdoc />
    public string[] Names { get; }

    /// <inheritdoc />
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public ConditionEvaluator[] Evaluators { get; }

    /// <inheritdoc />
    public string? Name
        => Names.Length > 0 ? Names[0] : null;

    /// <inheritdoc />
    public bool IsSearchable
        => Names.Length > 0;

    /// <inheritdoc />
    public bool IsDefault
        => false;

    /// <summary>
    ///     Gets the type of this module.
    /// </summary>
#if NET8_0_OR_GREATER
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
    public Type? Type { get; }

    /// <summary>
    ///     Gets the depth of the module, being how deeply nested it is in the component manager.
    /// </summary>
    public int Depth
        => Parent?.Depth + 1 ?? 1;

    internal CommandGroup(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type, CommandGroup? parent, string[] names, ComponentConfiguration configuration)
        : base(configuration.GetProperty("MakeModulesReadonly", false))
    {
        Parent = parent;
        Type = type;

        var attributes = type.GetAttributes(true);

        Attributes = attributes.ToArray();
        Evaluators = ConditionEvaluator.CreateEvaluators(attributes.OfType<ICondition>()).Concat(parent?.Evaluators ?? []).ToArray();

        Names = names;

        Activator = new CommandGroupActivator(type);

        Push(configuration.BuildNestedComponents(this).OrderByDescending(x => x.GetScore()));
    }

    internal CommandGroup(
        CommandGroup? parent, ICondition[] conditions, string[] names, ComponentConfiguration configuration)
        : base(configuration.GetProperty("MakeModulesReadonly", false))
    {
        Parent = parent;

        Attributes = [];
        Evaluators = ConditionEvaluator.CreateEvaluators(conditions).Concat(parent?.Evaluators ?? []).ToArray();

        Names = names;
    }

    /// <inheritdoc />
    public string GetFullName()
    {
        var sb = new StringBuilder();

        if (Parent != null && Parent.Name != null)
        {
            sb.Append(Parent.GetFullName());

            if (Name != null)
                sb.Append(' ');

            sb.Append(Name);
        }
        else if (Name != null)
            sb.Append(Name);

        return sb.ToString();
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
    public bool HasAttribute<T>()
        where T : Attribute
        => Attributes.Contains<T>(true);

    /// <inheritdoc />
    public T? GetAttribute<T>(T? defaultValue = default)
        where T : Attribute
        => Attributes.FirstOrDefault<T>() ?? defaultValue;

    /// <inheritdoc />
    public int CompareTo(object? obj)
        => obj is ICommandSegment scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public bool Equals(IComponent? other)
        => other is CommandGroup info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override IEnumerable<SearchResult> Find(ArgumentArray args)
    {
        var discovered = new List<SearchResult>()
        {
            SearchResult.FromError(this)
        };

        var index = Depth;

        foreach (var component in this)
        {
            if (component.IsDefault)
                discovered.Add(SearchResult.FromSuccess(component, index));

            if (!args.TryGetElementAt(index, out var value) || !component.Names.Contains(value))
                continue;

            if (component is CommandGroup group)
                discovered.AddRange(group.Find(args));
            else
                discovered.Add(SearchResult.FromSuccess(component, index + 1));
        }

        return discovered;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();

        if (Parent != null)
        {
            sb.Append(Parent.ToString());
            sb.Append('.');
        }

        // When type is null this group has been created manually.
        // We can only assume it is a group, as it does not have a type.
        if (Type == null)
            sb.Append("Group");
        else
            sb.Append(Type.Name);

        if (Name != null)
        {
            sb.Append("['");
            sb.Append(Name);
            sb.Append("']");
        }

        return sb.ToString();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is CommandGroup info && ReferenceEquals(this, info);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    // When a command is not yet bound to a parent, it can be bound when it is added to a CommandGroup. If it is added to a ComponentManager, it will not be bound.
    void IComponent.Bind(CommandGroup parent)
        => Parent ??= parent;

    /// <inheritdoc cref="Create(string[], ComponentConfiguration?)"/>
    public static CommandGroup Create(params string[] names)
        => Create(names, null);

    /// <summary>
    ///    Creates a new empty command group from the provided names and configuration.
    /// </summary>
    /// <param name="names">A set of names by which the command group will be able to be discovered. If this group is added to a <see cref="ComponentManager"/> (making it a top-level component), no names need to be provided.</param>
    /// <param name="configuration">The configuration that should be used to configure the built component.</param>
    /// <returns>A new <b>empty</b> instance of <see cref="CommandGroup"/>, able to be mutated using exposed API's.</returns>
    public static CommandGroup Create(string[] names, ComponentConfiguration? configuration = null)
    {
        configuration ??= ComponentConfiguration.Default;

        Assert.NotNull(names, nameof(names));
        Assert.Names(names, configuration, true);

        return new CommandGroup(null, [], names, configuration);
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
    ///     Creates a new command group from the provided type and configuration. If the provided type is a nested type, it will not be considered nested for the creation of this <see cref="CommandGroup"/>.
    /// </summary>
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
            throw new ArgumentException($"Type must be non-abstract, fully implemented, and assignable to {nameof(CommandModule)} to be considered a valid group.", nameof(type));

        var names = type.GetAttributes(false).FirstOrDefault<NameAttribute>()?.Names ?? [];

        Assert.Names(names, configuration, true);

        return new CommandGroup(type, null, names, configuration);
    }

    /// <summary>
    ///     Creates a new <see cref="CommandGroupBuilder"/> which can be built into a new instance of <see cref="CommandGroup"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="CommandGroupBuilder"/> containing API's to configure a <see cref="CommandGroup"/> with specified behavior.</returns>
    public static CommandGroupBuilder CreateBuilder()
        => new();
}
