using Commands.Parsing;
using Commands.Results;

namespace Commands.Resolvers
{
    internal sealed class DefaultSourceResolver : SourceResolverBase
    {
        public override async ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (Ready())
            {
                Console.CursorVisible = true;
                Console.Write("> ");

                var src = Console.ReadLine()!;

                Console.CursorVisible = false;

                return Success(new ConsumerBase(), StringParser.ParseKeyValueCollection(src));
            }

            return Error(new InvalidOperationException("The application failed to start."));
        }
    }
}
