using Commands.Resolvers;

namespace Commands.Tests
{
    internal class CustomSourceResolver : SourceResolver
    {
        public override ValueTask<SourceResult> Evaluate(IServiceProvider services, CancellationToken cancellationToken)
        {
            if (Ready())
            {
                Console.CursorVisible = true;
                Console.Write("> ");

                var src = Console.ReadLine()!;

                Console.CursorVisible = false;

                return Success(new CallerContext(), CommandParser.ParseKeyCollection(src));
            }

            return Error(new InvalidOperationException("The application failed to start."));
        }
    }
}
