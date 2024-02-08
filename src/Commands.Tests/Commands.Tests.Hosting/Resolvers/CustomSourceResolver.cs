using Commands.Core;
using Commands.Parsing;
using Commands.Resolvers;
using Commands.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Tests
{
    internal class CustomSourceResolver : SourceResolverBase
    {
        public override ValueTask<SourceResult> EvaluateAsync(CancellationToken cancellationToken)
        {
            var src = Console.ReadLine();

            Console.WriteLine(src);

            return ValueTask.FromResult(Success(new ConsumerBase(), StringParser.Parse(src)));   
        }
    }
}
