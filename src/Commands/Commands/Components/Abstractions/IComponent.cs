using System.ComponentModel;

namespace Commands;

/// <summary>
///     Reveals information about a conditional component, needing validation in order to become part of execution.
/// </summary>
public interface IComponent : ICommandSegment, IComparable<IComponent>
{
    /// <summary>
    ///     Gets the parent group of this component.
    /// </summary>
    /// <remarks>
    ///      This property can be <see langword="null"/> when the component is bound to a <see cref="ComponentTree"/> rather than a <see cref="CommandGroup"/>.
    /// </remarks>
    public CommandGroup? Parent { get; }

    /// <summary>
    ///     Gets an array of names for this component.
    /// </summary>
    public string[] Names { get; }

    /// <summary>
    ///     Gets if the component should be ignored during the execution process.
    /// </summary>
    public bool Ignore { get; }

    /// <summary>
    ///     Gets if the component name is queryable.
    /// </summary>
    public bool IsSearchable { get; }

    /// <summary>
    ///     Gets if the component is the default of a module-layer.
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>
    ///     Gets the position of the component, being how deeply nested it is in the component manager.
    /// </summary>
    public int Position { get; }
}

/// <inheritdoc/>
internal interface IInternalComponent : IComponent
{
    /// <summary>
    ///     Sets the parent group of the component.
    /// </summary>
    /// <param name="parent">The new parent group of the component. When the set is a <see cref="CommandGroup"/>, it is set as the <see cref="IComponent.Parent"/> of the component. Otherwise, it is considered bound.</param>
    /// <exception cref="ComponentFormatException">The component is already bound to a group or tree.</exception>
    public void Bind(ComponentSet parent);

    /// <summary>
    ///     Unbinds the component from its parent group, if it is bound.
    /// </summary>
    public void Unbind();
}