
namespace Commands.Samples;

public class SampleContext(string username, string? args) : ConsoleCallerContext(args)
{
    public string Name { get; } = username;

    public override void Respond(object? response)
        => Console.WriteLine($"[{Name}]: {response}");
}
