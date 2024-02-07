using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commands.Conditions;

namespace Commands.Core
{
    public struct RequestContext()
    {
        /// <summary>
        ///     Gets or sets the approach to asynchronousity in command execution.
        /// </summary>
        /// <remarks>
        ///     If set to <see cref="AsyncApproach.Await"/>, the manager will wait for a command to finish before allowing another to be executed.
        ///     If set to <see cref="AsyncApproach.Discard"/>, the manager will seperate the command execution from the entry stack, and slip it to another thread. 
        ///     Only change this value if you have read the documentation of <see cref="Core.AsyncApproach"/> and understand the definitions.
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
