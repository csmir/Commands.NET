
namespace Commands;

/// <inheritdoc cref="IComponentSet"/>
[DebuggerDisplay("Count = {Count}")]
public abstract class ComponentSet : IComponentSet
{
    private IComponent[] _items = [];

    internal Action<IEnumerable<IComponent>, bool>? _mutateTree;

    /// <inheritdoc />
    public int Count
        => _items.Length;

    /// <inheritdoc />
    public abstract IComponent[] Find(Arguments args);

    /// <inheritdoc />
    public bool Contains(IComponent component)
        => _items.Contains(component);

    /// <inheritdoc />
    public IEnumerable<Command> GetCommands(bool browseNestedComponents = true)
        => GetCommands(_ => true, browseNestedComponents);

    /// <inheritdoc />
    public IEnumerable<Command> GetCommands(Predicate<Command> predicate, bool browseNestedComponents = true)
    {
        Assert.NotNull(predicate, nameof(predicate));

        if (!browseNestedComponents)
            return _items.OfType(predicate);

        List<Command> discovered = [];

        foreach (var component in _items)
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
            return _items.OfType(predicate);

        List<CommandGroup> discovered = [];

        foreach (var component in _items)
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
        foreach (var component in _items)
        {
            if (component is CommandGroup grp)
                sum += grp.CountAll();

            sum++;
        }

        return sum;
    }

    /// <inheritdoc />
    public void Add(IComponent component)
        => AddRange([component]);

    /// <inheritdoc />
    public int AddRange(params IComponent[] components)
        => AddRange((IEnumerable<IComponent>)components);

    /// <inheritdoc />
    public int AddRange(IEnumerable<IComponent> components)
    {
        lock (_items)
        {
            var additions = FilterComponents(components);

            if (additions.Count > 0)
            {
                var copy = new IComponent[_items.Length + additions.Count];

                _items.CopyTo(copy, 0);

                for (int i = 0; i < additions.Count; i++)
                    copy[_items.Length + i] = additions[i];

                Array.Sort(copy);

                _mutateTree?.Invoke(components, false);
                _items = copy;
            }

            return additions.Count;
        }
    }

    /// <inheritdoc />
    public bool Remove(IComponent component)
        => RemoveRange([component]) > 0;

    /// <inheritdoc />
    public int RemoveRange(params IComponent[] components)
        => RemoveRange((IEnumerable<IComponent>)components);

    /// <inheritdoc />
    public int RemoveRange(IEnumerable<IComponent> components)
    {
        lock (_items)
        {
            var mutations = 0;

            var copy = new List<IComponent>(_items);

            foreach (var component in components)
            {
                Assert.NotNull(component, nameof(component));

                var output = copy.Remove(component);

                if (output)
                {
                    mutations += 1;
                    component.Unbind();
                }
            }

            if (mutations > 0)
            {
                _mutateTree?.Invoke(components, true);
                _items = [.. copy];
            }

            return mutations;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_items)
            _items = [];
    }

    /// <inheritdoc />
    public void CopyTo(IComponent[] array, int arrayIndex)
        => _items.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<IComponent> GetEnumerator()
        => new StateEnumerator(this);

    /// <summary>
    ///     An enumerator for the current state of the collection.
    /// </summary>
    /// <remarks>
    ///     This enumerator does not reflect changes made to the collection after the enumerator was created, nor does it reject iterations after modifications to the root collection.
    /// </remarks>
    public struct StateEnumerator : IEnumerator<IComponent>
    {
        private readonly IComponent[] _items;
        private int _index;
        private IComponent? _current;

        /// <inheritdoc />
        public readonly IComponent Current
            => _current!;

        internal StateEnumerator(ComponentSet collection)
        {
            _items = new IComponent[collection._items.Length];
            _index = 0;
            _current = default;

            collection._items.CopyTo(_items, 0);
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_index < _items.Length)
            {
                _current = _items[_index];
                _index++;
                return true;
            }
            _index = _items.Length;
            _current = default;
            return false;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _index = 0;
            _current = default;
        }

        /// <inheritdoc />
        public readonly void Dispose() { }

        readonly object IEnumerator.Current
            => Current;
    }

    #region Internals

    // Gets an stale enumerator which copies the current state of the collection into a span and iterates it.
    internal SpanStateEnumerator GetSpanEnumerator()
        => new(this);

    // Returns which of the provided components should be added to the collection.
    private List<IComponent> FilterComponents(IEnumerable<IComponent> components)
    {
        var discovered = new List<IComponent>();

        foreach (var component in components)
        {
            Assert.NotNull(component, nameof(component));

            if (_items.Contains(component))
                continue;

            if (this is ComponentTree rootSet)
            {
                // When a component is not searchable it means it has no names. Between a manager and a group, a different restriction applies to how this should be done.
                if (!component.IsSearchable)
                {
                    // Anything added to the manager should be considered top-level.
                    // Because a command realistically can never be executed if it has no name, we reject it from being added.
                    if (component is not CommandGroup group)
                        throw new InvalidOperationException($"{nameof(Command)} instances without names can only be added to a {nameof(CommandGroup)}.");

                    discovered.AddRange(FilterComponents(group._items));
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
                        throw new InvalidOperationException($"{nameof(CommandGroup)} instances without names can only be added to a {nameof(ComponentTree)}.");

                    discovered.Add(component);
                }
                else
                    discovered.Add(component);
            }

            component.Bind(this);
        }

        return discovered;
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    bool ICollection<IComponent>.IsReadOnly
        => false;

    internal ref struct SpanStateEnumerator
    {
        private readonly Span<IComponent> _items;

        private int _index;

        // We convert this to a non-nullable so we do not need to propagate null checks all over the codebase.
        public IComponent Current;

        internal SpanStateEnumerator(ComponentSet collection)
        {
            _index = 0;
            Current = null!;

            _items = collection._items.AsSpan();
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_index < _items.Length)
            {
                Current = _items[_index];
                _index++;
                return true;
            }

            _index = _items.Length;
            Current = null!;

            return false;
        }
    }

    // This method is used to add a component to the array of components with low allocation overhead.
    internal static void Yield(ref IComponent[] array, IComponent component)
    {
        var newArray = new IComponent[array.Length + 1];

        Array.Copy(array, newArray, array.Length);

        newArray[array.Length] = component;

        array = newArray;
    }

    // This method is used to add a range of components to the array of components with low allocation overhead.
    internal static void Yield(ref IComponent[] array, IComponent[] components)
    {
        var newArray = new IComponent[array.Length + components.Length];

        Array.Copy(array, newArray, array.Length);

        var i = array.Length;

        foreach (var component in components)
            newArray[i++] = component;

        array = newArray;
    }

    #endregion
}
