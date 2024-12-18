namespace Commands.Conversion
{
    /// <summary>
    ///     A collection type which is converted by a collection converter. Non-generic collections are not supported.
    /// </summary>
    public enum CollectionType
    {
        /// <summary>
        ///     The collection type is not a collection.
        /// </summary>
        None,

        /// <summary>
        ///     The collection type is a <see cref="System.Array"/>.
        /// </summary>
        Array = 1,

        /// <summary>
        ///     The collection type is a <see cref="List{T}"/>, <see cref="IList{T}"/>, <see cref="IReadOnlyList{T}"/>, <see cref="ICollection{T}"/>, <see cref="IReadOnlyCollection{T}"/> or <see cref="IEnumerable{T}"/>.
        /// </summary>
        List = 2,

        /// <summary>
        ///     The collection type is a <see cref="HashSet{T}"/> or <see cref="ISet{T}"/>.
        /// </summary>
        /// <remarks>
        ///     <see cref="SortedSet{T}"/> is not supported from this type.
        /// </remarks>
        Set = 3
    }
}
