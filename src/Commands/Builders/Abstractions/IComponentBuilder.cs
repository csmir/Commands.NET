namespace Commands.Builders;

/// <summary>
///     A builder model for a searchable component.
/// </summary>
public interface IComponentBuilder
{
    /// <summary>
    ///     Gets or sets all names of the component, including its name. This is used to identify the component in the command execution pipeline.
    /// </summary>
    public ICollection<string> Names { get; }

    /// <summary>
    ///     Gets or sets the conditions necessary for the component to execute.
    /// </summary>
    public ICollection<ExecuteCondition> Conditions { get; set; }

    /// <summary>
    ///     Builds a searchable component from the provided configuration.
    /// </summary>
    /// <param name="configuration">The configuration which sets the component up for execution. If not provided, <see cref="ComponentConfiguration.Default"/> will be used instead.</param>
    /// <returns>A reflection-based container that holds information about the component as configured using this builder.</returns>
    public IComponent Build(ComponentConfiguration? configuration = null);
}
