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

        internal CallerContext? Consumer { get; }

        internal ArgumentEnumerator? Args { get; }

        private SourceResult(CallerContext? consumer, ArgumentEnumerator? args, CommandOptions? options, Exception? exception)
        {
            Options = options;
            Consumer = consumer;
            Args = args;
            Exception = exception;
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="args">An unparsed command query, which will be parsed using <see cref="CommandParser.ParseKeyValueCollection(string)"/>.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(CallerContext consumer, string args, CommandOptions? options = null)
        {
            var parseResult = CommandParser.ParseKeyValueCollection(args);

            options ??= new CommandOptions();

            return new(consumer, new ArgumentEnumerator(parseResult, options.MatchComparer), options, null);
        }

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(CallerContext consumer, IEnumerable<object> args, CommandOptions? options = null)
            => new(consumer, new ArgumentEnumerator(args), options, null);

        /// <summary>
        ///     Creates a new <see cref="SourceResult"/> resembling a successful sourcing operation.
        /// </summary>
        /// <param name="consumer">The consumer of the command.</param>
        /// <param name="args">A parsed command query.</param>
        /// <param name="options">A set of options that determine logic in the command execution.</param>
        /// <returns>A new result containing information about the operation.</returns>
        public static SourceResult FromSuccess(CallerContext consumer, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
            => new(consumer, new ArgumentEnumerator(args, options?.MatchComparer ?? StringComparer.OrdinalIgnoreCase), options, null);

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
