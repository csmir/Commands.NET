using Commands.Conditions;
using Commands.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    internal class ORConditionAttribute(bool pass) : PreconditionAttribute<OREvaluator>
    {
        public override ValueTask<ConditionResult> Evaluate(ConsumerBase consumer, CommandInfo command, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (pass)
                return ValueTask.FromResult(Success());

            return ValueTask.FromResult(Error("The condition failed."));
        }
    }
}
