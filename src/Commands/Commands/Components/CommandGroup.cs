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
    /// <inheritdoc cref="CommandGroup(Type, CommandGroup?, ComponentOptions?)" />
    public CommandGroup(CommandGroup? parent = null, ComponentOptions? options = null)
        : base(typeof(T), parent, options) { }
}

/// <summary>
///     A concurrently accessible set of components, where <see cref="CommandGroup"/> instances are branches and <see cref="Command"/> instances are leaves.
/// </summary>
/// <remarks>
///     A group is a recursively browsable collection of <see cref="Command"/>, or subsequent <see cref="CommandGroup"/> instances. Browsing its branches is done using the <see cref="Find(Arguments)"/> method.
///     The group can be added to a <see cref="ComponentTree"/> or another <see cref="CommandGroup"/>. If added to a <see cref="ComponentTree"/>, it will be considered a top-level group. 
///     <br/>
///     Top level groups are not required to have <see cref="Names"/>, trimming the parent and accessing its members directly by their respective names. 
///     Components of a nameless top-level group are required to be named. If added to another <see cref="CommandGroup"/>, it will be bound to that group, and is required to have <see cref="Names"/>.
/// </remarks>
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

    /// <inheritdoc cref="CommandGroup(string[], ComponentOptions?)"/>
    public CommandGroup(params string[] names)
        : this(names, null) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="names">The names used to discover this group during execution.</param>
    /// <param name="options">An optional configuration containing additional settings when creating this command.</param>
    /// <exception cref="ArgumentException">The provided <paramref name="names"/> is <see langword="null"/> or does not match the <see cref="ComponentOptions.NameValidation"/> if any.</exception>
    public CommandGroup(string[] names, ComponentOptions? options = null)
    {
        Assert.NotNullOrInvalid(names, (options ?? ComponentOptions.Default).NameValidation, nameof(names));

        Ignore = false;
        Attributes = [];
        Names = names;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="type">The implementation of <see cref="CommandModule"/> that holds commands to be executed.</param>
    /// <param name="parent">The parent of this group, if any. Irrespective of this value being set, the group can still be added to groups at any time. This parameter will however, inherit the execution conditions from the parent.</param>
    /// <param name="options">An optional configuration containing additional settings when creating this command.</param>
    /// <exception cref="ArgumentNullException">The provided <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The provided <paramref name="type"/> defines names, but those names do not match the provided <see cref="ComponentOptions.NameValidation"/>.</exception>
    /// <exception cref="InvalidCastException">The provided type is not an implementation of <see cref="CommandModule"/>.</exception>
    public CommandGroup(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type, CommandGroup? parent = null, ComponentOptions? options = null)
    {
        options ??= ComponentOptions.Default;

        Assert.NotNull(type, nameof(type));

        if (!typeof(CommandModule).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
            throw new InvalidCastException($"The provided type is not an implementation of {nameof(CommandModule)}.");

        Parent = parent;

        var attributes = type.GetAttributes(true);

        Attributes = [.. attributes];

        var names = attributes.FirstOrDefault<NameAttribute>()?.Names ?? [];

        Assert.NotNullOrInvalid(names, options.NameValidation, nameof(NameAttribute));

        Names = names;
        Ignore = attributes.Contains<IgnoreAttribute>();

        Activator = new CommandModuleActivator(type);

        if (!Ignore)
        {
            var components = ComponentUtilities.GetNestedComponents(options, this);

            AddRange(components);
        }
    }

    /// <summary>
    ///     Gets the conditions that determine whether the underlying command within this group can execute or not.
    /// </summary>
    /// <returns>An enumerable representing any conditions to be executed prior to method execution to determine whether the underlying command can be executed.</returns>
    public IEnumerable<ICondition> GetConditions()
    {
        if (Parent != null)
            return [.. Attributes.OfType<ICondition>(), .. Parent.GetConditions()];
        else
            return Attributes.OfType<ICondition>();
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
    public override IComponent[] Find(Arguments args)
    {
        IComponent[] discovered = [this];

        var enumerator = GetSpanEnumerator();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current.IsDefault)
                Yield(ref discovered, enumerator.Current);
            else
            {
                if (!args.TryGetElementAt(Position, out var value) || !enumerator.Current.Names.Contains(value))
                    continue;

                if (enumerator.Current is CommandGroup group)
                    Yield(ref discovered, group.Find(args));
                else
                    Yield(ref discovered, enumerator.Current);
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
