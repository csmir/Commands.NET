﻿namespace Commands.Tests;

internal class CustomSourceResolver : SourceProvider
{
    public override ValueTask<SourceResult> Receive(IServiceProvider services, CancellationToken cancellationToken)
    {
        if (Ready())
        {
            Console.CursorVisible = true;
            Console.Write("> ");

            var src = Console.ReadLine()!;

            Console.CursorVisible = false;

            return Success(new CallerContext(), ArgumentReader.Read(src));
        }

        return Error(new InvalidOperationException("The application failed to start."));
    }

    public class CallerContext : ICallerContext
    {
        public Task Respond(object? response)
        {
            Console.WriteLine(response);

            return Task.CompletedTask;
        }
    }
}
