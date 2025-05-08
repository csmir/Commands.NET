using System.ComponentModel;

namespace Commands;

/// <summary>
///     Represents a set of properties that can be used to create a <see cref="IComponent"/>.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IComponentBuilder
{
    /// <summary>
    ///     Converts the properties to an implementation of<see cref="IComponent"/>.
    /// </summary>
    /// <param name="configuration">The configuration object to configure this object during creation.</param>
    /// <returns>A new implementation of <see cref="IComponent"/>.</returns>
    public IComponent Build(ComponentConfiguration? configuration = null);
}
