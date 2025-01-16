using System.ComponentModel;

namespace Commands;

/// <summary>
///     Represents a set of properties that can be used to create a <see cref="IComponent"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IComponentProperties
{
    /// <summary>
    ///     Converts the properties to an implementation of<see cref="IComponent"/>.
    /// </summary>
    /// <param name="parent">The parent object of this group. If left as null, the group will not inherit any configured values of said parent, such as conditions.</param>
    /// <param name="configuration">The configuration object to configure this object during creation.</param>
    /// <returns>A new implementation of <see cref="IComponent"/>.</returns>
    public IComponent Create(CommandGroup? parent = null, ComponentConfiguration? configuration = null);
}
