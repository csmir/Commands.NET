using System;
using System.Collections.Generic;
using System.Text;

namespace Commands;

/// <summary>
///     Represents an object that can contain attributes.
/// </summary>
public interface IAttributeContainer
{
    /// <summary>
    ///     Gets all attributes on the current object.
    /// </summary>
    public Attribute[] Attributes { get; }
}
