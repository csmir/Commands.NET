﻿namespace Commands;

/// <summary>
///     An attribute that defines the name of a module (<see cref="CommandModule"/>), a declared command or a command parameter.
/// </summary>
/// <remarks>
///     This attribute defines the name of a top-level component as well as all its members. 
///     If a <see cref="CommandModule"/> is named and its invokable members (command methods) are not, they will take on the name of the module instead, serving as default overloads.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
public class NameAttribute : Attribute, INameBinding
{
    /// <summary>
    ///     Gets the name of the target.
    /// </summary>
    public string Name
        => Names[0];

    /// <summary>
    ///     Gets the names of the target.
    /// </summary>
    /// <remarks>
    ///     Names are not considered for parameter names. Parameters have only one name, defined by <see cref="Name"/>.
    /// </remarks>
    public string[] Names { get; }

    /// <summary>
    ///     Creates a new <see cref="NameAttribute"/> with defined name.
    /// </summary>
    /// <param name="name">The target name.</param>
    public NameAttribute(string name)
        : this([name]) { }

    /// <summary>
    ///     Creates a new <see cref="NameAttribute"/> with defined name and names.
    /// </summary>
    /// <param name="names">The target's names. Names are not considered for parameter names.</param>
    /// <exception cref="ArgumentException">Thrown when no names are provided.</exception>
    public NameAttribute(params string[] names)
        => Names = names;
}
