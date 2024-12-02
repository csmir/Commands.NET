using Commands.Reflection;

namespace Commands
{
    /// <summary>
    ///     Represents a builder for a searchable component.
    /// </summary>
    public interface IComponentBuilder
    {
        /// <summary>
        ///     Builds a searchable component from the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration which sets the component up for execution.</param>
        /// <returns>A reflection-based container that holds information for a component ready to be executed or serves as a container for executable components.</returns>
        public ISearchable Build(CommandConfiguration configuration);
    }
}
