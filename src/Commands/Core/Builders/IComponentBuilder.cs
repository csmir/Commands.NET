using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     Represents a builder for a searchable component.
    /// </summary>
    public interface IComponentBuilder
    {
        /// <summary>
        ///     Gets all aliases of the command, including its name. This is used to identify the command in the command execution pipeline.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Builds a searchable component from the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration which sets the component up for execution.</param>
        /// <returns>A reflection-based container that holds information for a component ready to be executed or serves as a container for executable components.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the component aliases do not match <see cref="BuildConfiguration.NamingRegex"/>.</exception>
        public ISearchable Build(BuildConfiguration configuration);
    }
}
