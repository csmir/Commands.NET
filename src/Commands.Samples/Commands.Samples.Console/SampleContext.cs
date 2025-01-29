
namespace Commands.Samples;

public class SampleContext(string username, string? args) : ConsoleContext(args)
{
    public string Name { get; } = username;

    public override void Respond(object? response)
        => Console.WriteLine($"[{Name}]: {response}");
}
