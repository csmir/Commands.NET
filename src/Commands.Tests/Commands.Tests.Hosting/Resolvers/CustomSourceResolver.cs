using Commands.Core;
using Commands.Parsing;
using Commands.Resolvers;
using Commands.Results;

namespace Commands.Tests
{
    internal class CustomSourceResolver : SourceResolverBase
    {
        public override ValueTask<SourceResult> EvaluateAsync(CancellationToken cancellationToken)
        {
            var src = Console.ReadLine();

            return ValueTask.FromResult(Success(new ConsumerBase(), StringParser.Parse(src)));
        }
    }
}
