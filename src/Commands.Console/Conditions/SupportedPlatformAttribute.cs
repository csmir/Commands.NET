﻿using Commands.Reflection;

namespace Commands.Conditions
{
    /// <summary>
    ///     Represents a precondition that checks if the current platform is supported.
    /// </summary>
    /// <param name="platform">The platform name to verify.</param>
    public sealed class SupportedPlatformAttribute(string platform) : PreconditionAttribute<OREvaluator>
    {
        /// <inheritdoc />
        public override ValueTask<ConditionResult> Evaluate(ConsumerBase consumer, CommandInfo command, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (OperatingSystem.IsOSPlatform(platform))
                return ValueTask.FromResult(Success());

            return ValueTask.FromResult(Error("The platform does not support this operation."));
        }
    }
}