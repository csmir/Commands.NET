﻿using Commands.Conditions;
using Commands.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Console.Conditions
{
    /// <summary>
    ///     Represents a precondition that checks if the current platform is supported.
    /// </summary>
    /// <param name="platform">The platform name to verify.</param>
    public sealed class UnsupportedPlatformAttribute(string platform) : PreconditionAttribute<ANDEvaluator>
    {
        /// <inheritdoc />
        public override ValueTask<ConditionResult> Evaluate(ConsumerBase consumer, CommandInfo command, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (OperatingSystem.IsOSPlatform(platform))
                return ValueTask.FromResult(Error("The platform does not support this operation."));

            return ValueTask.FromResult(Success());
        }
    }
}