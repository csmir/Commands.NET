namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals a name and potential attributes of a component necessary for execution.
    /// </summary>
    public interface INameable
    {
        /// <summary>
        ///     Gets the name of the component.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets an array of attributes of this component.
        /// </summary>
        public Attribute[] Attributes { get; }
    }
}
