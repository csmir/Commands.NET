﻿using System.Diagnostics;

namespace Commands.Core
{
    /// <summary>
    ///     The result of a convert operation within the command execution pipeline.
    /// </summary>
    [DebuggerDisplay("Success = {Success()}")]
    public readonly struct ConvertResult : IRunResult
    {
        /// <inheritdoc />
        public Exception Exception { get; } = null;

        /// <inheritdoc />
        public bool Success
        {
            get
            {
                return Exception == null;
            }
        }

        internal object Value { get; } = null;

        internal ConvertResult(object value)
        {
            Value = value;
        }

        internal ConvertResult(Exception exception)
        {
            Exception = exception;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Success = {(Exception == null ? "True" : $"False \nException = {Exception}")}";
        }
    }
}
