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
        : base()
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
        CommandGroup? parent, IEnumerable<ExecuteCondition> conditions, string[] names)
        : base()
    {
        Parent = parent;

        var currentConditions = ConditionEvaluator.CreateEvaluators(conditions);

        if (parent != null)
            Evaluators = [.. currentConditions, .. parent.Evaluators];
        else
            Evaluators = [.. currentConditions];

        Attributes = [];
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

        var enumerator = GetSpanEnumerator();

        while (enumerator.MoveNext())
            score += enumerator.Current!.GetScore();

        if (Name != Type?.Name)
            score += 1.0f;

        score += Attributes.FirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

        return score;
    }

    /// <inheritdoc />
    public int CompareTo(IComponent? component)
        => GetScore().CompareTo(component?.GetScore());

    /// <inheritdoc />
    public override IEnumerable<IComponent> Find(ArgumentArray args)
    {
        List<IComponent> discovered = [this];

        var enumerator = GetSpanEnumerator();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.IsDefault)
                discovered.Add(enumerator.Current);
            else
            {
                if (!args.TryGetElementAt(Position, out var value) || !enumerator.Current.Names.Contains(value))
                    continue;

                if (enumerator.Current is CommandGroup group)
                    discovered.AddRange(group.Find(args));
                else
                    discovered.Add(enumerator.Current);
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

    int IComparable.CompareTo(object? obj)
        => obj is IComponent component ? CompareTo(component) : -1;

    #region Initializers

    /// <inheritdoc cref="From(string[])"/>
    public static CommandGroupProperties With
        => new();

    /// <summary>
    ///     Defines a collection of properties to configure and convert into a new instance of <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="names">A set of names this group be discovered by.</param>
    /// <returns>A fluent-pattern property object that can be converted into an instance when configured.</returns>
    public static CommandGroupProperties From(params string[] names)
        => new CommandGroupProperties().Names(names);

    #endregion
}
