namespace Commands;

/// <summary>
///     Represents a concurrent, recursive collection of components that can be searched and filtered based on their type.
/// </summary>
public interface IComponentCollection : ICollection<IComponent>, IEnumerable<IComponent>
{
    /// <summary>
    ///     Gets the depth of the current collection, being how deeply nested it is in the component manager.
    /// </summary>
    /// <remarks>
    ///     The depth of the root collection, being the <see cref="IExecutionProvider"/> implementing this collection, is 0.
    /// </remarks>
    public int Depth { get; }

    /// <summary>
    ///     Searches recursively through this and all subcollections for components that match the provided arguments.
    /// </summary>
    /// <param name="args">The arguments to base the search operation on.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands of this operation.</returns>
    public abstract IEnumerable<SearchResult> Find(ArgumentArray args);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="Command"/>.
    /// </summary>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their commands.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
    public IEnumerable<IComponent> GetCommands(bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="Command"/>, matching the provided predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate to retrieve commands with.</param>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their commands.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
    public IEnumerable<IComponent> GetCommands(Predicate<Command> predicate, bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their modules.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered modules in this operation.</returns>
    public IEnumerable<IComponent> GetModules(bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="CommandGroup"/>, matching the provided predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate to retrieve modules with.</param>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their modules.</param>    
    /// <returns>A lazily evaluated enumerable containing the discovered modules in this operation.</returns>
    public IEnumerable<IComponent> GetModules(Predicate<CommandGroup> predicate, bool browseNestedComponents = true);

    /// <summary>
    ///     Retrieves the count of all subcomponents in the current collection of components, including their own subcomponents. The result is the amount of commands and modules available in this collection.
    /// </summary>
    /// <returns>A recursive sum of all components available to the current collection.</returns>
    public int CountAll();

    /// <summary>
    ///     Adds all provided components to the current collection.
    /// </summary>
    /// <param name="components">The components to be added to the collection.</param>
    /// <returns>The number of added components, being 0 if no records were added.</returns>
    public int AddRange(params IComponent[] components);

    /// <summary>
    ///     Removes a component from the current collection if it exists.
    /// </summary>
    /// <param name="component">The component to be removed from the collection.</param>
    /// <returns><see langword="true"/> if the component was removed; Otherwise, <see langword="false"/>.</returns>
    public new bool Remove(IComponent component);

    /// <summary>
    ///     Removes all provided components from the current collection.
    /// </summary>
    /// <param name="components">The components to be removed from the collection.</param>
    /// <returns>The number of removed components, being 0 if no commands were removed.</returns>
    public int RemoveRange(params IComponent[] components);

    /// <summary>
    ///     Adds a component to the current collection.
    /// </summary>
    /// <param name="component">The component to be added to the module.</param>
    /// <returns><see langword="true"/> if the component was added; Otherwise, <see langword="false"/>.</returns>
    public new bool Add(IComponent component);
}
