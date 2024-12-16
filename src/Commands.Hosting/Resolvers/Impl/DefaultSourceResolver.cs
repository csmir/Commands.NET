namespace Commands.Resolvers
{
    internal sealed class DefaultSourceResolver : SourceResolver
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

                return Success(new CallerContext(), CommandParser.ParseKeyValueCollection(src));
            }

            return Error(new InvalidOperationException("The application failed to start."));
        }
    }
}
