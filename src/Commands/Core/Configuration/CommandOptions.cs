﻿using Commands.Conditions;
using Microsoft.Extensions.DependencyInjection;

namespace Commands
{
    /// <summary>
    ///     A set of options for handling command queries and determining the workflow in the command scope.
    /// </summary>
    public sealed class CommandOptions()
    {
        /// <summary>
        ///     Gets or sets the scope for running the request.
        /// </summary>
        /// <remarks>
        ///     If not set, this value will be automatically populated by the <see cref="IServiceProvider"/> injected into the <see cref="CommandManager"/>.
        ///     <br/>
        ///     <br/>
        ///     Default: <see langword="default"/>
        /// </remarks>
        public IServiceScope Scope { get; internal set; } = null!;

        /// <summary>
        ///     Gets or sets the approach to asynchronousity in command execution.
        /// </summary>
        /// <remarks>
        ///     If set to <see cref="AsyncMode.Await"/>, the manager will wait for a command to finish before allowing another to be executed.
        ///     If set to <see cref="AsyncMode.Discard"/>, the manager will seperate the command execution from the entry stack, and slip it to another thread. 
        ///     Only change this value if you have read the documentation of <see cref="AsyncMode"/> and understand the definitions.
        ///     <br/>
        ///     <br/>
        ///     Default: <see cref="AsyncMode.Default"/> (await).
        /// </remarks>
        public AsyncMode AsyncMode { get; set; } = AsyncMode.Default;

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
