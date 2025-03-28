﻿using System.ComponentModel;

namespace Commands;

/// <summary>
///     Reveals information about a conditional component, needing validation in order to become part of execution.
/// </summary>
public interface IComponent : ICommandSegment, IComparable<IComponent>
{
    /// <summary>
    ///     Gets the parent group of this component. This property can be <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    ///     Sets the parent group of the component, if it is not yet set.
    /// </summary>
    /// <param name="parent">The new parent group of the component.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Bind(CommandGroup parent);
}
