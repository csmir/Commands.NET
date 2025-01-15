using Commands.Conditions;
using System.Text;

namespace Commands;

/// <summary>
///     Reveals information about a command module, hosting zero-or-more commands.
/// </summary>
[DebuggerDisplay("Count = {Count}, {ToString()}")]
public sealed class CommandGroup : ComponentCollection, IComponent
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

    /// <inheritdoc />
    public int Position
        => (Parent?.Position ?? 0) + (Name == null ? 0 : 1);

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
        Evaluators = ConditionEvaluator.CreateEvaluators(attributes.OfType<IExecuteCondition>()).Concat(parent?.Evaluators ?? []).ToArray();

        Names = names;

        Activator = new CommandGroupActivator(type);

        Push(configuration.BuildNestedComponents(this).OrderByDescending(x => x.GetScore()));
    }

    internal CommandGroup(
        CommandGroup? parent, IEnumerable<ExecuteCondition> conditions, string[] names, ComponentConfiguration configuration)
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
    public int CompareTo(object? obj)
        => obj is ICommandSegment scoreable ? GetScore().CompareTo(scoreable.GetScore()) : -1;

    /// <inheritdoc />
    public override IEnumerable<IComponent> Find(ArgumentArray args)
    {
        List<IComponent> discovered = [this];

        foreach (var component in this)
        {
            if (component.IsDefault)
                discovered.Add(component);
            else
            {
                if (!args.TryGetElementAt(Position, out var value) || !component.Names.Contains(value))
                    continue;

                if (component is CommandGroup group)
                    discovered.AddRange(group.Find(args));
                else
                    discovered.Add(component);
            }
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

    // When a command is not yet bound to a parent, it can be bound when it is added to a CommandGroup. If it is added to a ComponentManager, it will not be bound.
    void IComponent.Bind(CommandGroup parent)
        => Parent ??= parent;

    public static CommandGroupProperties Define()
        => new();

    public static CommandGroupProperties Define(params string[] names)
        => new CommandGroupProperties().Names(names);
}
