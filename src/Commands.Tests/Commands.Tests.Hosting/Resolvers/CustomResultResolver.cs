using Commands.Core;
using Commands.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    internal class CustomResultResolver : ResultResolverBase
    {
        public override ValueTask EvaluateAsync(ConsumerBase consumer, IRunResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
            Console.WriteLine(result);

            return ValueTask.CompletedTask;
        }
    }
}
