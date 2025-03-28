﻿
namespace Commands;

/// <summary>
///     A set of properties of a command group.
/// </summary>
public sealed class CommandGroupProperties : IComponentProperties
{
    private readonly List<IComponentProperties> _components;
    private readonly List<string> _names;

    /// <summary>
    ///     Creates a new instance of <see cref="CommandGroupProperties"/>.
    /// </summary>
    public CommandGroupProperties()
    {
        _components = [];
        _names      = [];
    }

    /// <summary>
    ///     Adds a name to the command group.
    /// </summary>
    /// <remarks>
    ///     This add-operation is case-insensitive.
    /// </remarks>
    /// <param name="name">The value to add. If this value already exists in the properties, it is ignored.</param>
    /// <returns>The same <see cref="CommandGroupProperties"/> for call-chaining.</returns>
    public CommandGroupProperties Name(string name)
    {
        Assert.NotNullOrEmpty(name, nameof(name));

        if (!_names.Contains(name, StringComparer.OrdinalIgnoreCase))
            _names.Add(name);

        return this;
    }

    /// <summary>
    ///     Adds multiple names to the command group.
    /// </summary>
    /// <remarks>
    ///     This add-operation is case-insensitive.
    /// </remarks>
    /// <param name="names">The values to add. If any value already exists in the properties, it is ignored.</param>
    /// <returns>The same <see cref="CommandGroupProperties"/> for call-chaining.</returns>
    public CommandGroupProperties Names(params string[] names)
    {
        foreach (var name in names)
            Name(name);

        return this;
    }

    /// <summary>
    ///     Adds a component to the command group.
    /// </summary>
    /// <param name="component">The component to add to the group.</param>
    /// <returns>The same <see cref="CommandGroupProperties"/> for call-chaining.</returns>
    public CommandGroupProperties Component(IComponentProperties component)
    {
        Assert.NotNull(component, nameof(component));

        _components.Add(component);

        return this;
    }

    /// <summary>
    ///     Adds multiple components to the command group.
    /// </summary>
    /// <param name="components">The components to add to the group.</param>
    /// <returns>The same <see cref="CommandGroupProperties"/> for call-chaining.</returns>
    public CommandGroupProperties Components(params IComponentProperties[] components)
    {
        foreach (var component in components)
            Component(component);

        return this;
    }

    /// <summary>
    ///     Converts the properties into a new instance of <see cref="CommandGroup"/> which implements all configured values.
    /// </summary>
    /// <param name="configuration">The configuration object to configure this object during creation.</param>
    /// <returns>A new instance of <see cref="CommandGroup"/>.</returns>
    public IComponent Create(ComponentConfiguration? configuration = null)
    {
        var group = new CommandGroup([.. _names]);

        if (_components.Count != 0)
            group.AddRange([.. _components.Select(component => component.Create(configuration))]);

        return group;
    }
}
