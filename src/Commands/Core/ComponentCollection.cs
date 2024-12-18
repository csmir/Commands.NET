using System.Collections;
using System.Diagnostics;

namespace Commands
{
    /// <summary>
    ///     An abstract implementation of a searchable concurrent collection, containing components that can be mutated and searched based on specific criteria.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public abstract class ComponentCollection : ICollection<IComponent>, IEnumerable<IComponent>
    {
        private readonly Action<IComponent[], bool>? _hierarchyRetentionHandler;

        private HashSet<IComponent> _components = [];

        /// <summary>
        ///     Gets the count of components in the current collection.
        /// </summary>
        public int Count
            => _components.Count;

        /// <summary>
        ///     Gets if the collection has been marked as readonly when created.
        /// </summary>
        public bool IsReadOnly { get; }

        internal ComponentCollection(object? isReadOnly, object? notifyTopLevelMutation)
        {
            if (notifyTopLevelMutation is Action<IComponent[], bool> notifyTopLevelMutationHandler)
                _hierarchyRetentionHandler = notifyTopLevelMutationHandler;

            if (isReadOnly is bool isReadOnlyHandler)
                IsReadOnly = isReadOnlyHandler;
        }

        /// <summary>
        ///     Searches recursively through this and all subcollections for components that match the provided arguments.
        /// </summary>
        /// <param name="args">The arguments to base the search operation on.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered commands of this operation.</returns>
        public abstract IEnumerable<SearchResult> Find(ArgumentEnumerator args);

        /// <summary>
        ///     Returns if the current collection contains the provided component.
        /// </summary>
        /// <param name="component">The component of which the equality is compared with the available components in the collection.</param>
        /// <returns><see langword="true"/> if the component is found; otherwise, <see langword="false"/>.</returns>
        public bool Contains(IComponent component)
            => _components.Contains(component);

        /// <summary>
        ///     Filters all components in the current collection that are of <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their commands.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
        public IEnumerable<IComponent> GetCommands(bool browseNestedComponents = true)
            => GetCommands(_ => true, browseNestedComponents);

        /// <summary>
        ///     Filters all components in the current collection that are of <see cref="CommandInfo"/>, matching the provided predicate.
        /// </summary>
        /// <param name="predicate">The filter predicate to retrieve commands with.</param>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their commands.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
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

        /// <summary>
        ///     Filters all components in the current collection that are of <see cref="ModuleInfo"/>.
        /// </summary>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their modules.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered modules in this operation.</returns>
        public IEnumerable<IComponent> GetModules(bool browseNestedComponents = true)
            => GetModules(_ => true, browseNestedComponents);

        /// <summary>
        ///     Filters all components in the current collection that are of <see cref="ModuleInfo"/>, matching the provided predicate.
        /// </summary>
        /// <param name="predicate">The filter predicate to retrieve modules with.</param>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their modules.</param>    
        /// <returns>A lazily evaluated enumerable containing the discovered modules in this operation.</returns>
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

        /// <summary>
        ///     Retrieves the count of all subcomponents in the current collection of components, including their own subcomponents. The result is the amount of commands and modules available in this collection.
        /// </summary>
        /// <returns>A recursive sum of all components available to the current collection.</returns>
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

        /// <summary>
        ///     Sorts the components in the current collection based on their score.
        /// </summary>
        public void Sort()
        {
            if (IsReadOnly)
                throw BuildException.AccessDenied();

            var orderedCopy = new HashSet<IComponent>(_components.OrderByDescending(x => x.Score));

            Interlocked.Exchange(ref _components, orderedCopy);
        }

        /// <summary>
        ///     Adds a component to the current collection.
        /// </summary>
        /// <param name="component">The component to be added to the module.</param>
        /// <returns><see langword="true"/> if the component was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="BuildException">Thrown when a collection is marked as read-only.</exception>
        public bool Add(IComponent component)
            => AddRange(component) > 0;

        /// <summary>
        ///     Adds all provided components to the current collection.
        /// </summary>
        /// <param name="components">The components to be added to the collection.</param>
        /// <returns>The number of added components, being 0 if no records were added.</returns>
        /// <exception cref="BuildException">Thrown when a collection is marked as read-only.</exception>
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

        /// <summary>
        ///     Removes a component from the current collection if it exists.
        /// </summary>
        /// <param name="component">The component to be removed from the collection.</param>
        /// <returns><see langword="true"/> if the component was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="BuildException">Thrown when the collection is marked as read-only.</exception>
        public bool Remove(IComponent component)
            => RemoveRange(component) > 0;

        /// <summary>
        ///     Removes all provided components from the current collection.
        /// </summary>
        /// <param name="components">The components to be removed from the collection.</param>
        /// <returns>The number of removed components, being 0 if no commands were removed.</returns>
        /// <exception cref="BuildException">Thrown when the collection is marked as read-only.</exception>
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

        /// <summary>
        ///     Removes all components from the current collection.
        /// </summary>
        /// <exception cref="BuildException">Thrown when the collection is marked as read-only.</exception>
        public void Clear()
        {
            if (IsReadOnly)
                throw BuildException.AccessDenied();

            Interlocked.Exchange(ref _components, []);
        }

        /// <summary>
        ///     Copies the components of the current collection to an array, starting at the provided index.
        /// </summary>
        /// <remarks>
        ///     Because this collection is not indexed, the components are copied in the order they were sorted in the collection, by default, the <see cref="IComponent.Score"/> of the components.
        /// </remarks>
        /// <param name="array">The array to which this collection should be copied.</param>
        /// <param name="arrayIndex">The index from which to start copying the current collection into the provided array.</param>
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
