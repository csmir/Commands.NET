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
    /// <inheritdoc cref="CommandGroup(Type, ComponentOptions?)" />
    public CommandGroup(ComponentOptions? options = null)
        : base(typeof(T), options) { }
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
public class CommandGroup : ComponentSet, IInternalComponent
{
    private bool _bound;

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
        => IsSearchable ? Names[0] : null;

    /// <inheritdoc />
    public bool IsSearchable
        => Names.Length > 0;

    /// <inheritdoc />
    public bool IsDefault
        => false;

    /// <inheritdoc />
    public int Position
        => (Parent?.Position ?? 0) + (IsSearchable ? 1 : 0);

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
        options ??= ComponentOptions.Default;

        Assert.NotNullOrInvalid(names, options.NameValidation, nameof(names));

        Ignore = false;
        Attributes = [];
        Names = names;

        options.BuildCompleted?.Invoke(this);
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="type">The implementation of <see cref="CommandModule"/> that holds commands to be executed.</param>
    /// <param name="options">An optional configuration containing additional settings when creating this command.</param>
    /// <exception cref="ArgumentNullException">The provided <paramref name="type"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The provided <paramref name="type"/> defines names, but those names do not match the provided <see cref="ComponentOptions.NameValidation"/>.</exception>
    /// <exception cref="ComponentFormatException">The provided type is not an implementation of <see cref="CommandModule"/>.</exception>
    public CommandGroup(
#if NET8_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicNestedTypes)]
#endif
        Type type, ComponentOptions? options = null)
    {
        options ??= ComponentOptions.Default;

        Assert.NotNull(type, nameof(type));

        if (!typeof(CommandModule).IsAssignableFrom(type) || type.IsAbstract || type.ContainsGenericParameters)
            throw new ComponentFormatException($"The provided type is not a valid implementation of {nameof(CommandModule)}. Ensure it is not abstract, and does not contain unimplemented generic parameters.");

        var attributes = type.GetAttributes(true);

        Attributes = [.. attributes];

        var names = attributes.FirstOrDefault<INameBinding>()?.Names ?? [];

        Assert.NotNullOrInvalid(names, options.NameValidation, nameof(INameBinding));

        Names = names;
        Ignore = attributes.Any(x => x is IgnoreAttribute);

        Activator = new CommandModuleActivator(type);

        if (!Ignore)
        {
            if (Activator!.Type == null)
                return;

            var members = Activator!.Type!.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            var commands = new Command[members.Length];

            for (var i = 0; i < members.Length; i++)
                commands[i] = new Command(members[i], options);

            try
            {
                var nestedTypes = Activator.Type.GetNestedTypes(BindingFlags.Public);

                var groups = Utilities.GetComponents(options, nestedTypes, true);

                AddRange([.. commands.Where(x => !x.Ignore), .. groups]);
            }
            catch
            {
                // Do nothing else, we can't access nested types.
                AddRange(commands);
            }
        }

        options.BuildCompleted?.Invoke(this);
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
        var sb = new ValueStringBuilder(stackalloc char[64]);

        if (Parent?.Name != null)
        {
            sb.Append(Parent.GetFullName());
            sb.Append(' ');
        }

        if (Name != null)
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
            score += enumerator.Current.GetScore();

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
                Utilities.CopyTo(ref discovered, enumerator.Current);
            else
            {
                if (!args.TryGetElementAt(Position, out var value) || !enumerator.Current.Names.Contains(value))
                    continue;

                if (enumerator.Current is CommandGroup group)
                    Utilities.CopyTo(ref discovered, group.Find(args));
                else
                    Utilities.CopyTo(ref discovered, enumerator.Current);
            }
        }

        return discovered;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new ValueStringBuilder(stackalloc char[64]);

        if (Parent != null)
        {
            sb.Append(Parent.ToString());
            sb.Append('.');
        }

        // When type is null this group has been created manually.
        // We can only assume it is a group, as it does not have a type.
        sb.Append(Activator?.Type?.Name ?? "Group");

        if (Name != null)
        {
            sb.Append("['");
            sb.Append(Name);
            sb.Append("']");
        }

        return sb.ToString();
    }

    #region Internals

    int IComparable.CompareTo(object? obj)
        => obj is IComponent component ? CompareTo(component) : -1;

    void IInternalComponent.Bind(ComponentSet parent)
    {
        if (_bound)
            throw new ComponentFormatException($"{this} is already bound to a {(Parent != null ? nameof(CommandGroup) : nameof(ComponentSet))}. Remove this component from the parent set before adding it to another.");

        if (parent is CommandGroup group)
            Parent = group;

        if (parent is ComponentTree tree && !IsSearchable)
            _mutateTree = (components, removing) =>
            {
                if (removing)
                    tree.UnbindRange(components);
                else
                    tree.BindRange(components, true);
            };

        _bound = true;
    }

    void IInternalComponent.Unbind()
    {
        if (_mutateTree != null)
            _mutateTree = null;
        else
            Parent = null;

        _bound = _mutateTree != null || Parent != null;
    }

    #endregion
}
