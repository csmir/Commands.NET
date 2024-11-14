using Commands.Parsing;
using Commands.Results;

namespace Commands.Resolvers
{
    internal sealed class DefaultSourceResolver : SourceResolverBase
    {
        public override ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            if (Ready())
            {
                Console.CursorVisible = true;
                Console.Write("> ");

                var src = Console.ReadLine()!;

                Console.CursorVisible = false;

                return ValueTask.FromResult(Success(new ConsumerBase(), StringParser.ParseKeyValueCollection(src)));
            }

            return ValueTask.FromResult(Error(new InvalidOperationException("The application failed to start.")));
        }
    }
}
