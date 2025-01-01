namespace Commands
{
    /// <summary>
    ///     The result of the source acquirement within the command execution pipeline.
    /// </summary>
    public readonly struct SourceResult : IExecuteResult
    {
        /// <inheritdoc />
        public Exception? Exception { get; }

        /// <inheritdoc />
        public bool Success
            => Exception == null;

        internal CommandOptions? Options { get; }

        internal ICallerContext? Consumer { get; }

        internal ArgumentEnumerator? Args { get; }

        private SourceResult(ICallerContext? consumer, ArgumentEnumerator? args, CommandOptions? options, Exception? exception)
        {
            Options = options;
            Consumer = consumer;
            Args = args;
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="caller">The caller of the command.</param>
        /// <param name="args">An unparsed command query, which will be parsed using the <see cref="ArgumentReader"/>.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(ICallerContext caller, string args, CommandOptions? options = null)
        {
            options ??= new CommandOptions();

#if NET8_0_OR_GREATER
            return new(caller, new ArgumentEnumerator(ArgumentReader.ReadNamed(args), options.Comparer), options, null);
#else
            return new(caller, new ArgumentEnumerator(ArgumentReader.Read(args)), options, null);
#endif
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="caller">The caller of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(ICallerContext caller, IEnumerable<object> args, CommandOptions? options = null)
            => new(caller, new ArgumentEnumerator(args), options, null);

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="caller">The caller of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(ICallerContext caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
            => new(caller, new ArgumentEnumerator(args, options?.Comparer ?? StringComparer.OrdinalIgnoreCase), options, null);

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a failed sourcing operation.
        /// </summary>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromError()
            => new(null, null, null, SourceException.GetSourceFailed());

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a failed sourcing operation.
        /// </summary>
        /// <param name="exception">An exception describing the failed process.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromError(Exception exception)
            => new(null, null, null, SourceException.GetSourceFailed(exception));

        /// <inheritdoc />
        public override string ToString()
            => $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";

        /// <summary>
        ///     Turns this result into a <see cref="ValueTask{TResult}"/> for asynchronous operations.
        /// </summary>
        /// <param name="result">The result object to wrap in a <see cref="ValueTask{TResult}"/>.</param>
        public static implicit operator ValueTask<SourceResult>(SourceResult result)
            => new(result);
    }
}
