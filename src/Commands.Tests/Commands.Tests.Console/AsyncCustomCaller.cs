namespace Commands.Tests;

public class AsyncCustomCaller : AsyncCallerContext
{
    public override Task Respond(object? response)
    {
        Console.WriteLine(response);

        return Task.CompletedTask;
    }
}
