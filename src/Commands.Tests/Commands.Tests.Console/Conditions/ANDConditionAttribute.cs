using Commands.Conditions;
using Commands.Reflection;

namespace Commands.Tests
{
    public class ANDConditionAttribute(bool pass) : ConditionAttribute<ANDEvaluator>
    {
        public override ValueTask<ConditionResult> Evaluate(CallerContext consumer, CommandInfo command, ConditionTrigger trigger, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (pass)
                return ValueTask.FromResult(Success());

            return ValueTask.FromResult(Error("The condition failed."));
        }
    }
}
