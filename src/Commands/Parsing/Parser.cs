namespace Commands.Parsing
{
    /// <summary>
    ///     An abstract type that implements a very basic API for implementing command query parsing.
    /// </summary>
    /// <remarks>
    ///     To benefit from the base implementation of this query parser, create a new <see cref="StringParser"/> and run <see cref="StringParser.Parse(string)"/>.
    /// </remarks>
    /// <typeparam name="T">The input type this parser should convert into arguments. This type must implement <see cref="IEquatable{T}"/>.</typeparam>
    public abstract class Parser<T>
        where T : IEquatable<T>
    {
        /// <summary>
        ///     Parses a raw object into object arguments based on guidelines defined by the implementation.
        /// </summary>
        /// <param name="value">The raw value to convert into arguments.</param>
        /// <returns>An array of objects converted by the parser.</returns>
        public abstract object[] Parse(T value);
    }
}
