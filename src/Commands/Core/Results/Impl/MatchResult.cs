﻿using Commands.Reflection;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a match operation within the command execution pipeline.
    /// </summary>
    public readonly struct MatchResult : ICommandResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <summary>
        ///     Gets the command known during the matching operation.
        /// </summary>
        public CommandInfo Command { get; }

        internal object[] Reads { get; } = null;

        internal MatchResult(CommandInfo command, object[] reads)
        {
            Command = command;
            Reads = reads;
        }

        internal MatchResult(CommandInfo command, Exception exception)
        {
            Command = command;

            Exception = exception;
        }

        /// <inheritdoc />
        public bool Success()
        {
            return Exception == null;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False: {Exception}")}";
        }
    }
}