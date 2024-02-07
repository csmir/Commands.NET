﻿using Commands.Conditions;
using Microsoft.Extensions.Logging;

namespace Commands.Core
{
    /// <summary>
    ///     A context for handling command queries and determining the workflow in the command scope.
    /// </summary>
    public struct RequestContext()
    {
        /// <summary>
        ///     Gets or sets the approach to asynchronousity in command execution.
        /// </summary>
        /// <remarks>
        ///     If set to <see cref="AsyncApproach.Await"/>, the manager will wait for a command to finish before allowing another to be executed.
        ///     If set to <see cref="AsyncApproach.Discard"/>, the manager will seperate the command execution from the entry stack, and slip it to another thread. 
        ///     Only change this value if you have read the documentation of <see cref="Core.AsyncApproach"/> and understand the definitions.
        ///     <br/>
        ///     <br/>
        ///     Default: <see cref="AsyncApproach.Default"/> (await).
        /// </remarks>
        public AsyncApproach AsyncApproach { get; set; } = AsyncApproach.Default;

        /// <summary>
        ///     Gets or sets a token that can be provided from a <see cref="CancellationTokenSource"/> and later used to cancel asynchronous execution
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="default"/>
        /// </remarks>
        public CancellationToken CancellationToken { get; set; } = default;

        /// <summary>
        ///     Gets or sets a logger that can log information about the command scope.
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="null"/>
        /// </remarks>
        public ILogger Logger { get; set; } = null;

        /// <summary>
        ///     Gets or sets whether the defined <see cref="PostconditionAttribute"/>'s for this execution should be ran.
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="false"/>
        /// </remarks>
        public bool SkipPostconditions { get; set; } = false;

        /// <summary>
        ///     Gets or sets whether the defined <see cref="PreconditionAttribute"/>'s for this execution should be ran.
        /// </summary>
        /// <remarks>
        ///     Default: <see langword="false"/>
        /// </remarks>
        public bool SkipPreconditions { get; set; } = false;
    }
}
