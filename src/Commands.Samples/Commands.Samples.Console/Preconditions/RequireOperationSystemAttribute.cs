// Documentation of this file can be found at: https://github.com/csmir/Commands.NET/wiki/Conditions.

using Commands.Conditions;

namespace Commands.Samples;

public class RequireOperatingSystemAttribute(PlatformID platform) : ConditionAttribute<ANDEvaluator>
{
    public PlatformID Platform { get; } = platform;

    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (Environment.OSVersion.Platform == Platform)
            return Success();

        return Error("The platform does not support this operation.");
    }
}
