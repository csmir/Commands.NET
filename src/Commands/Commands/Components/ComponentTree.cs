namespace Commands;

/// <summary>
///     A concurrently accessible, rooted set of components, where <see cref="CommandGroup"/> instances are branches and <see cref="Command"/> instances are leaves. This class cannot be inherited.
/// </summary>
/// <remarks>
///     Execute items in this tree by implementing the instance into a <see cref="ComponentProvider"/> or by writing a custom execution pipeline.
/// </remarks>
[DebuggerDisplay("Count = {Count}")]
public sealed class ComponentTree : ComponentSet
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ComponentTree"/>.
    /// </summary>
    public ComponentTree() { }

    /// <summary>
    ///     Initializes a new instance of <see cref="ComponentTree"/> using the specified components and configuration.
    /// </summary>
    /// <param name="components">The components to add to this tree.</param>
    public ComponentTree(IEnumerable<IComponent> components) 
        => AddRange(components);

    /// <inheritdoc />
    public override IComponent[] Find(Arguments args)
    {
        IComponent[] discovered = [];

        var enumerator = GetSpanEnumerator();

        while (enumerator.MoveNext())
        {
            if (!args.TryGetElementAt(0, out var value) || !enumerator.Current.Names.Contains(value))
                continue;

            if (enumerator.Current is CommandGroup group)
                Utilities.CopyTo(ref discovered, group.Find(args));
            else
                Utilities.CopyTo(ref discovered, enumerator.Current);
        }

        return discovered;
    }

    /// <summary>
    ///     Attempts to add a type to the tree, using the <paramref name="options"/> to create a new <see cref="IComponent"/> implementation.
    /// </summary>
    /// <remarks>
    ///     This operation will add an implementation type of <see cref="CommandModule"/> that is public and non-abstract to the current tree.
    ///     Any type that does not implement either of these base types will be ignored.
    /// </remarks>
    /// <param name="type">The type to add to the tree, if possible.</param>
    /// <param name="options">Options to use when creating the components.</param>
    /// <returns><see langword="true"/> if the component was created and succesfully added; otherwise <see langword="false"/>.</returns>
    public bool Add(Type type, ComponentOptions? options = null)
        => AddRange([type], options) > 0;

    /// <summary>
    ///     Attempts to add the provided type to the tree, using the <paramref name="options"/> to create a new <see cref="IComponent"/> implementation.
    /// </summary>
    /// <remarks>
    ///     This operation will not add the provided type if it is not a public, non-abstract implementation of <see cref="CommandModule"/>.
    /// </remarks>
    /// <typeparam name="T">The type implementation of <see cref="CommandModule"/> to add.</typeparam>
    /// <param name="options">Options to use when creating the components.</param>
    /// <returns><see langword="true"/> if the component was created and succesfully added; otherwise <see langword="false"/>.</returns>
    public bool Add<T>(ComponentOptions? options = null)
        where T : CommandModule
        => Add(typeof(T), options);

    /// <summary>
    ///     Attempts to add all provided types to the tree, using the <paramref name="options"/> to create new <see cref="IComponent"/> implementations.
    /// </summary>
    /// <remarks>
    ///     This operation will add implementation types of <see cref="CommandModule"/> that are public and non-abstract to the current tree. 
    ///     Any types that do not implement either of these base types will be ignored.
    /// </remarks>
    /// <param name="types">A collection of types to filter and add to the manager, where possible.</param>
    /// <param name="options">The options to use when creating the components.</param>
    /// <returns>The number of added components; or 0 if no components are added.</returns>
    public int AddRange(IEnumerable<Type> types, ComponentOptions? options = null)
    {
        options ??= ComponentOptions.Default;

        var components = types.GetComponents(options);

        return AddRange(components);
    }
}
