using Commands.Conditions;

namespace Commands.Tests;

public class ANDAttribute(bool pass) : ConditionAttribute<ANDEvaluator>
{
    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (pass)
            return Success();

        return Error("The condition failed.");
    }
}

public class OR1Attribute(bool pass) : ConditionAttribute<OREvaluator>
{
    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (pass)
            return Success();

        return Error("The condition failed.");
    }
}

public class OR2Attribute(bool pass) : ConditionAttribute<OREvaluator>
{
    public override ValueTask<ConditionResult> Evaluate(ICallerContext caller, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (pass)
            return Success();

        return Error("The condition failed.");
    }
}
