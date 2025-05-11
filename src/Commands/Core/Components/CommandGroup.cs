using Commands.Conditions;
using System.Text;

namespace Commands;

/// <inheritdoc cref="CommandGroup"/>
/// <typeparam name="T">The type of the <see cref="CommandModule"/> or <see cref="CommandModule{T}"/> implementation to consider a group of commands.</typeparam>
[DebuggerDisplay("Count = {Count}, {ToString()}")]
public class CommandGroup<
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
# endif
T> : CommandGroup
where T : CommandModule
{
    /// <inheritdoc cref="CommandGroup(Type, CommandGroup?, BuildOptions?)" />
    public CommandGroup(CommandGroup? parent = null, BuildOptions? options = null)
        : base(typeof(T), parent, options) { }
}

/// <summary>
///     A concurrently accessible set of components, where <see cref="CommandGroup"/> instances are branches and <see cref="Command"/> instances are leaves.
/// </summary>
[DebuggerDisplay("Count = {Count}, {ToString()}")]
public class CommandGroup : ComponentSet, IComponent
{
    /// <inheritdoc />
    public CommandGroup? Parent { get; private set; }

    /// <inheritdoc />
    public IDependencyActivator<CommandModule>? Activator { get; }

    /// <inheritdoc />
    public string[] Names { get; }

    /// <inheritdoc />
    public Attribute[] Attributes { get; }

    /// <inheritdoc />
    public bool Ignore { get; }

    /// <inheritdoc />
    public string? Name
        => Names.Length > 0 ? Names[0] : null;

    /// <inheritdoc />
    public bool IsSearchable
        => Names.Length > 0;

    /// <inheritdoc />
    public bool IsDefault
        => false;

    /// <inheritdoc />
    public int Position
        => (Parent?.Position ?? 0) + (Name == null ? 0 : 1);

    /// <summary>
    ///     Initializes a new instance of <see cref="CommandGroup"/> using the specified conditions, names and parent group.
    /// </summary>
    /// <param name="names">The names used to discover this group during execution.</param>
    public CommandGroup(params string[] names)
    {
        Ignore = false;
        Attributes = [];
        Names = names;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="CommandGroup"/> using the specified type and parent group.
    /// </summary>
    /// <param name="type">The implementation of <see cref="CommandModule"/> that holds commands to be executed.</param>
    /// <param name="parent">The parent of this group, if any. Irrespective of this value being set, the group can still be added to groups at any time. This parameter will however, inherit the execution conditions from the parent.</param>
    /// <param name="options">An optional configuration containing additional settings when creating this command.</param>
    /// <exception cref="InvalidCastException">The provided type is not an implementation of <see cref="CommandModule"/>.</exception>
    public CommandGroup(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type, CommandGroup? parent = null, BuildOptions? options = null)
    {
        if (!typeof(CommandModule).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
            throw new InvalidCastException($"The provided type is not an implementation of {nameof(CommandModule)}.");

        Parent = parent;

        var attributes = type.GetAttributes(true);

        Attributes = [.. attributes];

        Ignore = attributes.Contains<IgnoreAttribute>();
        Names = attributes.FirstOrDefault<NameAttribute>()?.Names ?? [];

        Activator = new CommandModuleActivator(type);

        if (!Ignore)
        {
            var components = ComponentUtilities.GetNestedComponents(options ?? BuildOptions.Default, this);

            AddRange(components);
        }
    }

    /// <summary>
    ///     Gets the conditions that determine whether the underlying command within this group can execute or not.
    /// </summary>
    /// <returns>An enumerable representing any conditions to be executed prior to method execution to determine whether the underlying command can be executed.</returns>
    public IEnumerable<IExecuteCondition> GetConditions()
    {
        if (Parent != null)
            return [.. Attributes.OfType<IExecuteCondition>(), .. Parent.GetConditions()];
        else
            return Attributes.OfType<IExecuteCondition>();
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

        if (Name != Activator?.Type?.Name)
            score += 1.0f;

        score += Attributes.FirstOrDefault<PriorityAttribute>()?.Priority ?? 0;

        return score;
    }

    /// <inheritdoc />
    public int CompareTo(IComponent? component)
        => GetScore().CompareTo(component?.GetScore());

    /// <inheritdoc />
    public override IEnumerable<IComponent> Find(Arguments args)
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
        if (Activator == null)
            sb.Append("Group");
        else
            sb.Append(Activator.Type?.Name);

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
}
