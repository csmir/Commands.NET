namespace Commands;

/// <summary>
///     A concurrently accessible, rooted set of components, where <see cref="CommandGroup"/> instances are branches and <see cref="Command"/> instances are leaves.
/// </summary>
/// <remarks>
///     Execute items in this tree by implementing the instance into a <see cref="ComponentProvider"/> or by writing a custom execution pipeline.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public sealed class ComponentTree : ComponentSet
{
    /// <summary>
    ///     Gets the configuration with which new additions to this tree will be created.
    /// </summary>
    public ComponentConfiguration Configuration { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="ComponentTree"/> using the default configuration.
    /// </summary>
    public ComponentTree() 
        => Configuration = ComponentConfiguration.Default;

    /// <summary>
    ///     Initializes a new instance of <see cref="ComponentTree"/> using the specified configuration.
    /// </summary>
    /// <param name="configuration">The configuration to configure newly created components added to this tree with.</param>
    public ComponentTree(ComponentConfiguration configuration)
    {
        Assert.NotNull(configuration, nameof(configuration));

        Configuration = configuration;
    }

    /// <summary>
    ///     Initializes a new instance of <see cref="ComponentTree"/> using the specified components and the default configuration.
    /// </summary>
    /// <param name="components">The components to add to this tree.</param>
    public ComponentTree(IEnumerable<IComponent> components)
        : this(components, ComponentConfiguration.Default) { }

    /// <summary>
    ///     Initializes a new instance of <see cref="ComponentTree"/> using the specified components and configuration.
    /// </summary>
    /// <param name="components">The components to add to this tree.</param>
    /// <param name="configuration">The configuration to configure newly created components added to this tree with.</param>
    public ComponentTree(IEnumerable<IComponent> components, ComponentConfiguration configuration)
        : this(configuration)
    {
        Assert.NotNull(components, nameof(components));

        AddRange(components);
    }

    /// <inheritdoc />
    public override IEnumerable<IComponent> Find(ArgumentDictionary args)
    {
        List<IComponent> discovered = [];

        var enumerator = GetSpanEnumerator();

        while (enumerator.MoveNext())
        {
            if (!args.TryGetElementAt(0, out var value) || !enumerator.Current.Names.Contains(value))
                continue;

            if (enumerator.Current is CommandGroup group)
                discovered.AddRange(group.Find(args));

            else
                discovered.Add(enumerator.Current);
        }

        return discovered;
    }

    /// <summary>
    ///     Adds a collection of types to the component manager.
    /// </summary>
    /// <remarks>
    ///     This operation will add implementations of <see cref="CommandModule"/> and <see cref="CommandModule{T}"/>, that are public and non-abstract to the current manager.
    /// </remarks>
    /// <param name="types">A collection of types to filter and add to the manager, where possible.</param>
    /// <returns>The number of added components; or 0 if no components are added.</returns>
    public int AddRange(IEnumerable<Type> types)
    {
        var components = ComponentUtilities.GetComponents(types, Configuration);
        return AddRange(components);
    }
}
