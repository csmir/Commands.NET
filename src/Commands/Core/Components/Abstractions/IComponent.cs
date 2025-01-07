using Commands.Conditions;
using System.ComponentModel;

namespace Commands;

/// <summary>
///     Reveals information about a conditional component, needing validation in order to become part of execution.
/// </summary>
public interface IComponent : ICommandSegment, IEquatable<IComponent>
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
    ///     Gets all evaluations that this component should do during the execution process, determined by a set of defined <see cref="IExecuteCondition"/>'s pointing at the component.
    /// </summary>
    /// <remarks>
    ///     When this property is called by a child component, this property will inherit all evaluations from the component's <see cref="Parent"/> component(s).
    /// </remarks>
    public ConditionEvaluator[] Evaluators { get; }

    /// <summary>
    ///     Gets the invocation target of this component.
    /// </summary>
    public IActivator? Activator { get; }

    /// <summary>
    ///     Gets if the component name is queryable.
    /// </summary>
    public bool IsSearchable { get; }

    /// <summary>
    ///     Gets if the component is the default of a module-layer.
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>
    ///     Sets the parent group of the component, if it is not yet set.
    /// </summary>
    /// <param name="parent">The new parent group of the component.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Bind(CommandGroup parent);
}
