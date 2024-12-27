﻿using Commands.Conditions;

namespace Commands.Tests
{
    public class ANDConditionAttribute(bool pass) : ConditionAttribute<ANDEvaluator>
    {
        public override Task<ConditionResult> Evaluate(ICallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (pass)
                return Success();

            return Error("The condition failed.");
        }
    }
}
