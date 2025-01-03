using System.Collections;

namespace Commands;

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
    public bool IsReadOnly { get; }

    internal ComponentCollection(bool isReadOnly)
        => IsReadOnly = isReadOnly;

    /// <inheritdoc />
    public abstract IEnumerable<SearchResult> Find(ArgumentEnumerator args);

    /// <inheritdoc />
    public bool Contains(IComponent component)
        => _components.Contains(component);

    /// <inheritdoc />
    public IEnumerable<IComponent> GetCommands(bool browseNestedComponents = true)
        => GetCommands(_ => true, browseNestedComponents);

    /// <inheritdoc />
    public IEnumerable<IComponent> GetCommands(Predicate<Command> predicate, bool browseNestedComponents = true)
    {
        if (!browseNestedComponents)
            return _components.Where(x => x is Command cmd && predicate(cmd));

        List<IComponent> discovered = [];

        foreach (var component in _components)
        {
            if (component is Command command && predicate(command))
                discovered.Add(command);

            if (component is CommandGroup module)
                discovered.AddRange(module.GetCommands(predicate, browseNestedComponents));
        }

        return discovered;
    }

    /// <inheritdoc />
    public IEnumerable<IComponent> GetModules(bool browseNestedComponents = true)
        => GetModules(_ => true, browseNestedComponents);

    /// <inheritdoc />
    public IEnumerable<IComponent> GetModules(Predicate<CommandGroup> predicate, bool browseNestedComponents = true)
    {
        if (!browseNestedComponents)
            return _components.Where(x => x is CommandGroup grp && predicate(grp));

        List<IComponent> discovered = [];

        foreach (var component in _components)
        {
            if (component is CommandGroup grp)
            {
                if (predicate(grp))
                    discovered.Add(component);

                discovered.AddRange(grp.GetModules(predicate, browseNestedComponents));
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
    /// <exception cref="NotSupportedException">Thrown when the collection is marked as read-only.</exception>
    public void Sort()
    {
        ThrowIfLocked();

        var orderedCopy = new HashSet<IComponent>(_components.OrderByDescending(x => x.Score));

        Interlocked.Exchange(ref _components, orderedCopy);
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Thrown when the collection is marked as read-only.</exception>
    public bool Add(IComponent component)
        => AddRange(component) > 0;

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Thrown when the collection is marked as read-only.</exception>
    public int AddRange(params IComponent[] components)
    {
        ThrowIfLocked();

        var hasChanged = 0;

        var copy = new HashSet<IComponent>(_components);

        foreach (var component in components)
            hasChanged += copy.Add(component) ? 1 : 0;

        if (hasChanged > 0)
        {
            // Notify the top-level collection that a mutation has occurred. This will add, and resort the components.
            _mutateParent?.Invoke(components, false);

            var orderedCopy = new HashSet<IComponent>(copy.OrderByDescending(x => x.Score));

            Interlocked.Exchange(ref _components, orderedCopy);
        }

        return hasChanged;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Thrown when the collection is marked as read-only.</exception>
    public bool Remove(IComponent component)
        => RemoveRange(component) > 0;

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Thrown when the collection is marked as read-only.</exception>
    public int RemoveRange(params IComponent[] components)
    {
        ThrowIfLocked();

        var copy = new HashSet<IComponent>(_components);
        var removed = 0;

        foreach (var component in components)
            removed += copy.Remove(component) ? 1 : 0;

        if (removed > 0)
        {
            _mutateParent?.Invoke(components, true);

            Interlocked.Exchange(ref _components, copy);
        }

        return removed;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Thrown when the collection is marked as read-only.</exception>
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
        {
            AddRange(components);
            Sort();
        }
    }

    // Throws an exception if the collection is marked as read-only.
    private void ThrowIfLocked()
    {
        if (IsReadOnly)
            throw new NotSupportedException("This collection has been marked as read-only and cannot be mutated.");
    }

    void ICollection<IComponent>.Add(IComponent item)
        => Add(item);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
