namespace Commands.Conversion
{
    /// <summary>
    ///     A converter that can convert a collection type.
    /// </summary>
    public interface ICollectionConverter
    {
        /// <summary>
        ///     Gets the collection type that this converter can convert.
        /// </summary>
        public CollectionType CollectionType { get; }
    }
}
