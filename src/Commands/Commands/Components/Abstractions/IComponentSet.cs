namespace Commands;

/// <summary>
///     Represents a concurrent, mutable and recursive set of components that can be searched and filtered based on arguments.
/// </summary>
public interface IComponentSet : ICollection<IComponent>, IEnumerable<IComponent>
{
    /// <summary>
    ///     Searches recursively through this and all subsets for components that match the provided arguments.
    /// </summary>
    /// <param name="args">The arguments to base the search operation on.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands of this operation.</returns>
    public IComponent[] Find(Arguments args);

    /// <summary>
    ///     Filters all components in the current set that are of <see cref="Command"/>.
    /// </summary>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their commands.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
    public IEnumerable<Command> GetCommands(bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current set that are of <see cref="Command"/>, matching the provided predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate to retrieve commands with.</param>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their commands.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered commands in this operation.</returns>
    public IEnumerable<Command> GetCommands(Predicate<Command> predicate, bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current set that are of <see cref="CommandGroup"/>.
    /// </summary>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their groups.</param>
    /// <returns>A lazily evaluated enumerable containing the discovered groups in this operation.</returns>
    public IEnumerable<CommandGroup> GetGroups(bool browseNestedComponents = true);

    /// <summary>
    ///     Filters all components in the current set that are of <see cref="CommandGroup"/>, matching the provided predicate.
    /// </summary>
    /// <param name="predicate">The filter predicate to retrieve groups with.</param>
    /// <param name="browseNestedComponents">Defines if all subcomponents of this set should also be scanned and return their groups.</param>    
    /// <returns>A lazily evaluated enumerable containing the discovered groups in this operation.</returns>
    public IEnumerable<CommandGroup> GetGroups(Predicate<CommandGroup> predicate, bool browseNestedComponents = true);

    /// <summary>
    ///     Retrieves the count of all subcomponents in the current set of components, including their own subcomponents. The result is the amount of commands and groups available in this set.
    /// </summary>
    /// <returns>A recursive sum of all components available to the current set.</returns>
    public int CountAll();

    /// <inheritdoc cref="AddRange(IEnumerable{IComponent})"/>
    public int AddRange(params IComponent[] components);

    /// <summary>
    ///     Adds all provided components to the current set.
    /// </summary>
    /// <param name="components">The components to be added to the set.</param>
    /// <returns>The number of added components, being 0 if no records were added.</returns>
    public int AddRange(IEnumerable<IComponent> components);

    /// <inheritdoc cref="RemoveRange(IEnumerable{IComponent})"/>
    public int RemoveRange(params IComponent[] components);

    /// <summary>
    ///     Removes all provided components from the current set.
    /// </summary>
    /// <param name="components">The components to be removed from the set.</param>
    /// <returns>The number of removed components, being 0 if no commands were removed.</returns>
    public int RemoveRange(IEnumerable<IComponent> components);
}
