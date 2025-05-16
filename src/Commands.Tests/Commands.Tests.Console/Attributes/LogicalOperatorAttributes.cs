﻿using Commands.Conditions;

namespace Commands.Tests;

public class ANDAttribute(bool pass) : ExecuteConditionAttribute<ANDEvaluator>
{
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (pass)
            return Success();

        return Error("The condition failed.");
    }
}

public class OR1Attribute(bool pass) : ExecuteConditionAttribute<OREvaluator>
{
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (pass)
            return Success();

        return Error("The condition failed.");
    }
}

public class OR2Attribute(bool pass) : ExecuteConditionAttribute<OREvaluator>
{
    public override ValueTask<ConditionResult> Evaluate(IContext context, Command command, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (pass)
            return Success();

        return Error("The condition failed.");
    }
}
