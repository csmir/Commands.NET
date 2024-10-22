using Commands.Conditions;
using Commands.Reflection;

namespace Commands.Tests
{
    public class ANDConditionAttribute(bool pass) : PreconditionAttribute<ANDEvaluator>
    {
        public override ValueTask<ConditionResult> Evaluate(ConsumerBase consumer, CommandInfo command, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (pass)
                return ValueTask.FromResult(Success());

            return ValueTask.FromResult(Error("The condition failed."));
        }
    }
}
