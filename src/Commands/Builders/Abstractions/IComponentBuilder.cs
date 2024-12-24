namespace Commands.Builders
{
    /// <summary>
    ///     Represents a builder for a searchable component.
    /// </summary>
    public interface IComponentBuilder
    {
        /// <summary>
        ///     Gets all aliases of the command, including its name. This is used to identify the command in the command execution pipeline.
        /// </summary>
        public ICollection<string> Aliases { get; }

        /// <summary>
        ///     Builds a searchable component from the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration which sets the component up for execution. If left as <see langword="null"/>, <see cref="ComponentConfiguration.Default"/> will be used instead.</param>
        /// <returns>A reflection-based container that holds information for a component ready to be executed or serves as a container for executable components.</returns>
        /// <exception cref="ArgumentNullException">Thrown when necessary values are not set.</exception>
        public IComponent Build(ComponentConfiguration? configuration = null);
    }
}
