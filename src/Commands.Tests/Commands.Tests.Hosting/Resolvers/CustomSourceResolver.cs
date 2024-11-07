using Commands.Parsing;
using Commands.Resolvers;
using Commands.Results;

namespace Commands.Tests
{
    internal class CustomSourceResolver : SourceResolverBase
    {
        public override ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            if (Ready())
            {
                Console.CursorVisible = true;
                Console.Write("> ");

                var src = Console.ReadLine()!;

                Console.CursorVisible = false;

                return ValueTask.FromResult(Success(new ConsumerBase(), StringParser.ParseKeyCollection(src)));
            }

            return ValueTask.FromResult(Error(new InvalidOperationException("The application failed to start.")));
        }
    }
}
