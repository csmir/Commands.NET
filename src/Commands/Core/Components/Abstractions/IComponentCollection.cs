namespace Commands;

/// <summary>
///     Represents a concurrent, recursive collection of components that can be searched and filtered based on their type.
/// </summary>
public interface IComponentCollection : ICollection<IComponent>, IEnumerable<IComponent>
{
    /// <summary>
    ///     Searches recursively through this and all subcollections for components that match the provided arguments.
    /// </summary>
    /// <param name="args">The arguments to base the search operation on.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands of this operation.</returns>
    public abstract IEnumerable<IComponent> Find(ArgumentArray args);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="Command"/>.
    /// </summary>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their commands.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
    public IEnumerable<Command> GetCommands(bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="Command"/>, matching the provided predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate to retrieve commands with.</param>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their commands.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
    public IEnumerable<Command> GetCommands(Predicate<Command> predicate, bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their groups.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered groups in this operation.</returns>
    public IEnumerable<CommandGroup> GetGroups(bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current collection that are of <see cref="CommandGroup"/>, matching the provided predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate to retrieve groups with.</param>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this collection should also be scanned and return their groups.</param>    
    /// <returns>A lazily evaluated enumerable containing the discovered groups in this operation.</returns>
    public IEnumerable<CommandGroup> GetGroups(Predicate<CommandGroup> predicate, bool browseNestedComponents = true);

    /// <summary>
    ///     Retrieves the count of all subcomponents in the current collection of components, including their own subcomponents. The result is the amount of commands and groups available in this collection.
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
    /// <param name="component">The component to be added to the group.</param>
    /// <returns><see langword="true"/> if the component was added; Otherwise, <see langword="false"/>.</returns>
    public new bool Add(IComponent component);
}
