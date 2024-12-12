using Commands.Parsing;

namespace Commands.Reflection
{
    /// <summary>
    ///     Contains a set of components that can be mutated and searched based on specific criteria.
    /// </summary>
    public interface IComponentSet : IEnumerable<ISearchable>
    {
        /// <summary>
        ///     Gets the count of components in the current set.
        /// </summary>
        public int Count { get; }

        /// <summary>
        ///     Searches recursively through this and all subcollections for components that match the provided arguments.
        /// </summary>
        /// <param name="args">The arguments to base the search operation on.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered commands of this operation.</returns>
        public IEnumerable<SearchResult> Find(ArgumentEnumerator args);

        /// <summary>
        ///     Filters all components in the current module that are of <see cref="CommandInfo"/>.
        /// </summary>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their commands.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
        public IEnumerable<ISearchable> GetCommands(bool browseNestedComponents = true);

        /// <summary>
        ///     Filters all components in the current module that are of <see cref="CommandInfo"/>, matching the provided predicate.
        /// </summary>
        /// <param name="predicate">The filter predicate to retrieve commands with.</param>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their commands.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
        public IEnumerable<ISearchable> GetCommands(Predicate<CommandInfo> predicate, bool browseNestedComponents = true);

        /// <summary>
        ///     Filters all components in the current module that are of <see cref="ModuleInfo"/>.
        /// </summary>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their modules.</param>
        /// <returns>A lazily evaluated enumerable containing the discovered modules in this operation.</returns>
        public IEnumerable<ISearchable> GetModules(bool browseNestedComponents = true);

        /// <summary>
        ///     Filters all components in the current module that are of <see cref="ModuleInfo"/>, matching the provided predicate.
        /// </summary>
        /// <param name="predicate">The filter predicate to retrieve modules with.</param>
        /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their modules.</param>    
        /// <returns>A lazily evaluated enumerable containing the discovered modules in this operation.</returns>
        public IEnumerable<ISearchable> GetModules(Predicate<ModuleInfo> predicate, bool browseNestedComponents = true);

        /// <summary>
        ///     Retrieves all subcomponents in the current set of components.
        /// </summary>
        /// <returns>An enumerable containing all subcomponents of the current set.</returns>
        public IEnumerable<ISearchable> GetAll();

        /// <summary>
        ///     Retrieves the count of all subcomponents in the current set of components, including their own subcomponents. The result is the amount of commands and modules available in this set.
        /// </summary>
        /// <returns>A recursive sum of all components available to the current set.</returns>
        public int CountAll();

        /// <summary>
        ///     Adds a component to the current module.
        /// </summary>
        /// <param name="component">The component to be added to the module.</param>
        /// <returns><see langword="true"/> if the component was added; otherwise, <see langword="false"/>.</returns>
        public bool Add(ISearchable component);

        /// <summary>
        ///     Adds all provided components to the current module.
        /// </summary>
        /// <param name="components">The components to be added to the module.</param>
        /// <returns>The number of added components, being 0 if no records were added.</returns>
        public int AddRange(params ISearchable[] components);

        /// <summary>
        ///     Removes a component from the current module if it exists.
        /// </summary>
        /// <param name="component">The component to be added to the module.</param>
        /// <returns><see langword="true"/> if the component was removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(ISearchable component);

        /// <summary>
        ///     Removes all components from the current module that match the predicate.
        /// </summary>
        /// <param name="predicate">The predicate which determines which items should be removed from the module.</param>
        /// <returns>The number of removed components, being 0 if no records were removed.</returns>
        public int RemoveWhere(Predicate<ISearchable> predicate);
    }
}
