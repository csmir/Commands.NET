namespace Commands.Samples;

public sealed class HostedCallerContext(string input) : ICallerContext
{
    public string? Input { get; } = input;

    public Task Respond(object? response)
        => new(() => Console.WriteLine(response));
}
