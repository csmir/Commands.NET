using Commands.Core;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals a name and potential attributes of a component necessary for execution.
    /// </summary>
    public interface IScoreable
    {
        /// <summary>
        ///     Gets the name of the component.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> when a command does not implement <see cref="CommandAttribute"/> but is member of a module that implements <see cref="GroupAttribute"/>.
        /// </remarks>
        public string? Name { get; }

        /// <summary>
        ///     Gets an array of attributes of the component.
        /// </summary>
        public Attribute[] Attributes { get; }

        /// <summary>
        ///     Generates the score of the component.
        /// </summary>
        /// <returns>A float representing the score of the component.</returns>
        public float GetScore();
    }
}
