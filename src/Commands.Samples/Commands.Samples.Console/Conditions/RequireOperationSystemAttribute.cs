﻿namespace Commands.Samples;

public class RequireOperatingSystemAttribute(PlatformID platform) : ExecuteConditionAttribute<ANDEvaluator>
{
    public PlatformID Platform { get; } = platform;

    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (Environment.OSVersion.Platform == Platform)
            return Success();

        return Error("The platform does not support this operation.");
    }
}
