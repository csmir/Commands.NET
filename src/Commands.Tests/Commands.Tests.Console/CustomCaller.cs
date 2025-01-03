namespace Commands.Tests;

public class CustomCaller : ICallerContext
{
    public int ArgumentCount = 0;

    public void Respond(object? response)
        => Console.WriteLine(response);
}
