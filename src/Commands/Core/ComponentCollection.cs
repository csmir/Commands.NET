using Commands.Core.Abstractions;
using System.Collections;

namespace Commands
{
    /// <inheritdoc cref="IComponentCollection"/>
    public abstract class ComponentCollection : IComponentCollection
    {
        private readonly Action<IComponent[], bool>? _hierarchyRetentionHandler;

        private HashSet<IComponent> _components = [];

        /// <inheritdoc />
        public int Count
            => _components.Count;

        /// <inheritdoc />
        public bool IsReadOnly { get; }

        internal ComponentCollection()
            : this(false, null) { }

        internal ComponentCollection(bool isReadOnly, Action<IComponent[], bool>? hierarchyRetentionHandler)
        {
            _hierarchyRetentionHandler = hierarchyRetentionHandler;
            IsReadOnly = isReadOnly;
        }

        /// <inheritdoc />
        public abstract IEnumerable<SearchResult> Find(ArgumentEnumerator args);

        /// <inheritdoc />
        public bool Contains(IComponent component)
            => _components.Contains(component);

        /// <inheritdoc />
        public IEnumerable<IComponent> GetCommands(bool browseNestedComponents = true)
            => GetCommands(_ => true, browseNestedComponents);

        /// <inheritdoc />
        public IEnumerable<IComponent> GetCommands(Predicate<CommandInfo> predicate, bool browseNestedComponents = true)
        {
            if (!browseNestedComponents)
                return _components.Where(x => x is CommandInfo info && predicate(info));

            List<IComponent> discovered = [];

            foreach (var component in _components)
            {
                if (component is CommandInfo command && predicate(command))
                    discovered.Add(command);

                if (component is ModuleInfo module)
                    discovered.AddRange(module.GetCommands(predicate, browseNestedComponents));
            }

            return discovered;
        }

        /// <inheritdoc />
        public IEnumerable<IComponent> GetModules(bool browseNestedComponents = true)
            => GetModules(_ => true, browseNestedComponents);

        /// <inheritdoc />
        public IEnumerable<IComponent> GetModules(Predicate<ModuleInfo> predicate, bool browseNestedComponents = true)
        {
            if (!browseNestedComponents)
                return _components.Where(x => x is ModuleInfo info && predicate(info));

            List<IComponent> discovered = [];

            foreach (var component in _components)
            {
                if (component is ModuleInfo module)
                {
                    if (predicate(module))
                        discovered.Add(component);

                    discovered.AddRange(module.GetModules(predicate, browseNestedComponents));
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
                if (component is ModuleInfo module)
                    sum += module.CountAll();

                sum++;
            }

            return sum;
        }

        /// <inheritdoc />
        public void Sort()
        {
            if (IsReadOnly)
                throw BuildException.AccessDenied();

            var orderedCopy = new HashSet<IComponent>(_components.OrderByDescending(x => x.Score));

            Interlocked.Exchange(ref _components, orderedCopy);
        }

        /// <inheritdoc />
        public bool Add(IComponent component)
            => AddRange(component) > 0;

        /// <inheritdoc />
        public int AddRange(params IComponent[] components)
        {
            if (IsReadOnly)
                throw BuildException.AccessDenied();

            var hasChanged = 0;

            var copy = new HashSet<IComponent>(_components);

            foreach (var component in components)
                hasChanged += (copy.Add(component) ? 1 : 0);

            if (hasChanged > 0)
            {
                // Notify the top-level collection that a mutation has occurred. This will add, and resort the components.
                _hierarchyRetentionHandler?.Invoke(components, false);

                var orderedCopy = new HashSet<IComponent>(copy.OrderByDescending(x => x.Score));

                Interlocked.Exchange(ref _components, orderedCopy);
            }

            return hasChanged;
        }

        /// <inheritdoc />
        public bool Remove(IComponent component)
            => RemoveRange(component) > 0;

        /// <inheritdoc />
        public int RemoveRange(params IComponent[] components)
        {
            if (IsReadOnly)
                throw BuildException.AccessDenied();

            var copy = new HashSet<IComponent>(_components);
            var removed = 0;

            foreach (var component in components)
                removed += (copy.Remove(component) ? 1 : 0);

            if (removed > 0)
            {
                _hierarchyRetentionHandler?.Invoke(components, true);

                Interlocked.Exchange(ref _components, copy);
            }

            return removed;
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (IsReadOnly)
                throw BuildException.AccessDenied();

            Interlocked.Exchange(ref _components, []);
        }

        /// <inheritdoc />
        public void CopyTo(IComponent[] array, int arrayIndex)
            => _components.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public IEnumerator<IComponent> GetEnumerator()
            => _components.GetEnumerator();

        // This method is used to push components into the collection without any validation. This is used for internal operations.
        internal void PushDangerous(IEnumerable<IComponent> components)
            => _components = [.. components];

        void ICollection<IComponent>.Add(IComponent item)
            => Add(item);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
