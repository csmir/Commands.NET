namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a parameter of a signature.
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        ///     Gets the type of this argument.
        /// </summary>
        /// <remarks>
        ///     The returned value is always underlying where available, ensuring converters do not attempt to convert a nullable type.
        /// </remarks>
        public Type Type { get; }

        /// <summary>
        ///     Gets the exposed type of this argument.
        /// </summary>
        /// <remarks>
        ///     The returned value will differ from <see cref="Type"/> if <see cref="IsNullable"/> is <see langword="true"/>.
        /// </remarks>
        public Type ExposedType { get; }

        /// <summary>
        ///     Gets if this argument is nullable or not.
        /// </summary>
        public bool IsNullable { get; }

        /// <summary>
        ///     Gets if this argument is optional or not.
        /// </summary>
        public bool IsOptional { get; }
    }
}
