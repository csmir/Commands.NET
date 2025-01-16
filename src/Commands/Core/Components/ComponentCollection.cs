namespace Commands;

/// <inheritdoc cref="IComponentCollection"/>
[DebuggerDisplay("Count = {Count}")]
public abstract class ComponentCollection : IComponentCollection
{
    private Action<IComponent[], bool>? _mutateParent;

    private HashSet<IComponent> _components;

    private readonly SemaphoreSlim _lock;

    /// <inheritdoc />
    public int Count
        => _components.Count;

    /// <inheritdoc />
    public bool IsReadOnly { get; }

    internal ComponentCollection(bool isReadOnly)
    {
        IsReadOnly = isReadOnly;

        _lock = new(1, 1);
        _components = [];
    }

    /// <inheritdoc />
    public abstract IEnumerable<IComponent> Find(ArgumentArray args);

    /// <inheritdoc />
    public bool Contains(IComponent component)
        => _components.Contains(component);

    /// <inheritdoc />
    public IEnumerable<Command> GetCommands(bool browseNestedComponents = true)
        => GetCommands(_ => true, browseNestedComponents);

    /// <inheritdoc />
    public IEnumerable<Command> GetCommands(Predicate<Command> predicate, bool browseNestedComponents = true)
    {
        Assert.NotNull(predicate, nameof(predicate));

        if (!browseNestedComponents)
            return _components.OfType(predicate);

        List<Command> discovered = [];

        foreach (var component in _components)
        {
            if (component is Command command && predicate(command))
                discovered.Add(command);

            if (component is CommandGroup grp)
                discovered.AddRange(grp.GetCommands(predicate, browseNestedComponents));
        }

        return discovered;
    }

    /// <inheritdoc />
    public IEnumerable<CommandGroup> GetGroups(bool browseNestedComponents = true)
        => GetGroups(_ => true, browseNestedComponents);

    /// <inheritdoc />
    public IEnumerable<CommandGroup> GetGroups(Predicate<CommandGroup> predicate, bool browseNestedComponents = true)
    {
        Assert.NotNull(predicate, nameof(predicate));

        if (!browseNestedComponents)
            return _components.OfType(predicate);

        List<CommandGroup> discovered = [];

        foreach (var component in _components)
        {
            if (component is CommandGroup grp)
            {
                if (predicate(grp))
                    discovered.Add(grp);

                discovered.AddRange(grp.GetGroups(predicate, browseNestedComponents));
            }
        }

        return discovered;
    }

    /// <inheritdoc />
    public int CountAll()
    {
        var sum = 0;
        foreach (var component in _components)
        {
            if (component is CommandGroup grp)
                sum += grp.CountAll();

            sum++;
        }

        return sum;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public bool Add(IComponent component)
        => AddRange(component) > 0;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public int AddRange(params IComponent[] components)
    {
        ThrowIfImmutable();

        lock (_components)
        {
            var copy = new HashSet<IComponent>(_components);

            var mutations = 0;

            // We do not use HashSet<T>.SymmetricExceptWith because we are already validating the components in the internal handler.
            // This is important, because we should not rebind components if we are not adding them.
            //
            // On another note, this method approaches the set via an elementary foreach clause anyway, it is not revolutionary.
            foreach (var component in ValidateAddition(components))
                mutations += copy.Add(component) ? 1 : 0;

            if (mutations > 0)
            {
                _mutateParent?.Invoke(components, false);

                _components = [.. copy.OrderByDescending(x => x.GetScore())];
            }

            return mutations;
        }
    }
    
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public bool Remove(IComponent component)
        => RemoveRange(component) > 0;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public int RemoveRange(params IComponent[] components)
    {
        ThrowIfImmutable();

        lock (_components)
        {
            var mutations = 0;

            foreach (var component in components)
            {
                Assert.NotNull(component, nameof(component));

                mutations += _components.Remove(component) ? 1 : 0;
            }

            if (mutations > 0)
                _mutateParent?.Invoke(components, true);

            return mutations;
        }
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public void Clear()
    {
        ThrowIfImmutable();

        lock (_components)
            _components.Clear();
    }

    /// <inheritdoc />
    public void CopyTo(IComponent[] array, int arrayIndex)
        => _components.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<IComponent> GetEnumerator()
        => _components.GetEnumerator();

    internal void Push(IEnumerable<IComponent> components)
        => _components = [.. components];

    // Returns which of the provided components should be added to the collection.
    private List<IComponent> ValidateAddition(IEnumerable<IComponent> components)
    {
        var discovered = new List<IComponent>();

        foreach (var component in components)
        {
            Assert.NotNull(component, nameof(component));

            if (_components.Contains(component))
                continue;

            if (this is ComponentManager manager)
            {
                // When a component is not searchable it means it has no names. Between a manager and a group, a different restriction applies to how this should be done.
                if (!component.IsSearchable)
                {
                    // Anything added to the manager should be considered top-level.
                    // Because a command realistically can never be executed if it has no name, we reject it from being added.
                    if (component is not CommandGroup group)
                        throw new InvalidOperationException($"{nameof(Command)} instances without names can only be added to a {nameof(CommandGroup)}.");

                    discovered.AddRange(ValidateAddition([.. group]));

                    // By binding a top-level group without a name to the manager, the manager will be notified of any changes made so it can update its state.
                    group.Bind(manager);
                }
                else
                    discovered.Add(component);
            }

            if (this is CommandGroup collection)
            {
                if (!component.IsSearchable)
                {
                    // Anything added to a group should be considered nested.
                    // Because of the nature of this design, we want to avoid folding anything but top level. This means that nested groups must be named.
                    if (component is not Command)
                        throw new InvalidOperationException($"{nameof(CommandGroup)} instances without names can only be added to a {nameof(ComponentManager)}.");

                    discovered.Add(component);
                }
                else
                    discovered.Add(component);

                // We always ensure the parent of the component is bound when it is added, ensuring that the component is able to access global state.
                component.Bind(collection);
            }
        }

        return discovered;
    }

    // Mutates the parent collection from the child collection, if bind is called by the parent collection.
    private void MutateFromChild(IComponent[] components, bool removing)
    {
        if (removing)
            RemoveRange(components);
        else
            AddRange(components);
    }

    // Throws an exception if the collection is marked as read-only.
    private void ThrowIfImmutable()
    {
        if (IsReadOnly)
            throw new InvalidOperationException("This collection has been marked as read-only and cannot be mutated.");
    }

    // Binds the parent collection to the child collection.
    private void Bind(ComponentCollection collection)
        => _mutateParent = collection.MutateFromChild;

    void ICollection<IComponent>.Add(IComponent item)
        => Add(item);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
