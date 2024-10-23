using Commands.Conditions;
using Commands.Reflection;

namespace Commands.Tests
{
    public class ORConditionAttribute(bool pass) : PreconditionAttribute<OREvaluator>
    {
        public override ValueTask<ConditionResult> Evaluate(ConsumerBase consumer, CommandInfo command, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (pass)
                return ValueTask.FromResult(Success());

            return ValueTask.FromResult(Error("The condition failed."));
        }
    }
}
