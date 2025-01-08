﻿namespace Commands;

/// <inheritdoc cref="IComponentCollection"/>
[DebuggerDisplay("Count = {Count}")]
public abstract class ComponentCollection : IComponentCollection
{
    private Action<IComponent[], bool>? _mutateParent;

    private HashSet<IComponent> _components = [];

    /// <inheritdoc />
    public int Count
        => _components.Count;

    /// <inheritdoc />
    public int Depth
        => this is CommandGroup group ? group.Position : 0;

    /// <inheritdoc />
    public bool IsReadOnly { get; }

    internal ComponentCollection(bool isReadOnly)
        => IsReadOnly = isReadOnly;

    /// <inheritdoc />
    public abstract IEnumerable<KeyValuePair<int, IComponent>> Find(ArgumentArray args);

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
            return _components.Where(x => x is Command cmd && predicate(cmd)).Cast<Command>();

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
            return _components.Where(x => x is CommandGroup grp && predicate(grp)).Cast<CommandGroup>();

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
        ThrowIfLocked();

        var hasChanged = 0;

        var copy = new HashSet<IComponent>(_components);

        foreach (var component in components)
        {
            Assert.NotNull(component, nameof(component));

            var additionResult = false;

            if (!component.IsSearchable && component is CommandGroup componentIsGroup)
            {
                // When the component is a command group and the group has no name, it should be a root group.
                // However, it is only possible to be a root group if the current collection is a manager.
                if (this is /* a */ ComponentManager) // Ingenius syntax, actually.
                {
                    // Add the contents of the group to the collection, instead of the group itself.
                    var innerChanges = AddRange([.. componentIsGroup]);

                    // Consider the addition successful if the inner operation returned more than 0 changes.
                    additionResult = innerChanges > 0;
                }
                else
                    throw new InvalidOperationException($"Nameless {nameof(CommandGroup)} instances can only be added to a {nameof(ComponentManager)}.");
            }
            else
            {
                additionResult = copy.Add(component);

                // When addition is successful, and this collection is a CommandGroup, bind the component to the group.
                if (this is CommandGroup collectionIsGroup && additionResult)
                    component.Bind(collectionIsGroup);
            }

            hasChanged += additionResult ? 1 : 0;
        }

        if (hasChanged > 0)
        {
            // Notify the top-level collection that a mutation has occurred. This will add, and re-sort the components.
            _mutateParent?.Invoke(components, false);

            var orderedCopy = new HashSet<IComponent>(copy.OrderByDescending(x => x.GetScore()));

            Interlocked.Exchange(ref _components, orderedCopy);
        }

        return hasChanged;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public bool Remove(IComponent component)
        => RemoveRange(component) > 0;

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public int RemoveRange(params IComponent[] components)
    {
        ThrowIfLocked();

        var copy = new HashSet<IComponent>(_components);
        var removed = 0;

        foreach (var component in components)
        {
            Assert.NotNull(component, nameof(component));

            removed += copy.Remove(component) ? 1 : 0;
        }

        if (removed > 0)
        {
            _mutateParent?.Invoke(components, true);

            Interlocked.Exchange(ref _components, copy);
        }

        return removed;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Thrown when the collection is marked as read-only.</exception>
    public void Clear()
    {
        ThrowIfLocked();

        Interlocked.Exchange(ref _components, []);
    }

    /// <inheritdoc />
    public void CopyTo(IComponent[] array, int arrayIndex)
        => _components.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<IComponent> GetEnumerator()
        => _components.GetEnumerator();

    internal void Bind(ComponentCollection collection)
        => _mutateParent = collection.MutateFromChild;

    internal void Push(IEnumerable<IComponent> components)
        => _components = [.. components];

    // Mutates the parent collection from the child collection, if bind is called by the parent collection.
    private void MutateFromChild(IComponent[] components, bool removing)
    {
        if (removing)
            RemoveRange(components);
        else
            AddRange(components);
    }

    // Throws an exception if the collection is marked as read-only.
    private void ThrowIfLocked()
    {
        if (IsReadOnly)
            throw new InvalidOperationException("This collection has been marked as read-only and cannot be mutated.");
    }

    void ICollection<IComponent>.Add(IComponent item)
        => Add(item);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
