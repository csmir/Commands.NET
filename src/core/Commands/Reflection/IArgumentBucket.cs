namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a bucket that contains zero-or-more arguments to resolve.
    /// </summary>
    public interface IArgumentBucket
    {
        /// <summary>
        ///     Gets an array of arguments this bucket exposes.
        /// </summary>
        public IArgument[] Arguments { get; }

        /// <summary>
        ///     Gets if this bucket has zero or more arguments.
        /// </summary>
        public bool HasArguments { get; }

        /// <summary>
        ///     Gets the minimum length of this bucket's arguments.
        /// </summary>
        public int MinLength { get; }

        /// <summary>
        ///     Gets the maximum length of this bucket's arguments.
        /// </summary>
        public int MaxLength { get; }
    }
}
