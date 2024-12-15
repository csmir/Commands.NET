// Documentation of this file can be found at: https://github.com/csmir/Commands.NET/wiki/Preconditions.

using Commands.Conditions;
using Commands.Reflection;

namespace Commands.Samples
{
    public class RequireOperatingSystemAttribute(PlatformID platform) : ConditionAttribute<ANDEvaluator>
    {
        public PlatformID Platform { get; } = platform;

        public override ValueTask<ConditionResult> Evaluate(CallerContext context, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (Environment.OSVersion.Platform == Platform)
                return ValueTask.FromResult(Success());

            return ValueTask.FromResult(Error("The platform does not support this operation."));
        }
    }
}
