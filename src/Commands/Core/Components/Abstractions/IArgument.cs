using Commands.Conversion;

namespace Commands
{
    /// <summary>
    ///     Reveals information about an invocation argument of a command or any complex member.
    /// </summary>
    public interface IArgument : IScorable, IParameter
    {
        /// <summary>
        ///     Gets the position of this argument in the command's parameter list.
        /// </summary>
        /// <remarks>
        ///     This value is zero-based.
        /// </remarks>
        public int Position { get; }

        /// <summary>
        ///     Gets if this argument is the query remainder or not.
        /// </summary>
        public bool IsRemainder { get; }

        /// <summary>
        ///     Gets if this argument is a collection type or not.
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        ///     Gets the parser for this argument.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if <see cref="Type"/> is <see cref="string"/>, <see cref="object"/>, or if this argument is <see cref="ComplexArgumentInfo"/>.
        /// </remarks>
        public TypeParser? Parser { get; }

        /// <summary>
        ///     Attempts to convert the provided value to the target type of this argument.
        /// </summary>
        /// <param name="caller">The caller of the current execution.</param>
        /// <param name="value">The value which the <see cref="Parser"/> should parse.</param>
        /// <param name="services">The provider used to register modules and inject services.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>An awaitable <see cref="Task{ConvertResult}"/> holding the result of the convert operation.</returns>
        public Task<ConvertResult> Parse(ICallerContext caller, object? value, IServiceProvider services, CancellationToken cancellationToken);
    }
}
