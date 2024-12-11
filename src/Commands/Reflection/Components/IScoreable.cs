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
        public string? Name { get; }

        /// <summary>
        ///     Gets an array of attributes of the component.
        /// </summary>
        public Attribute[] Attributes { get; }

        /// <summary>
        ///     Builds the score of the component.
        /// </summary>
        /// <returns>A float representing the score of the component.</returns>
        public float GetScore();
    }
}
