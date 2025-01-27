
namespace Commands.Samples;

public class ConsoleCallerContext(string name, string? args) : ConsoleContext(args)
{
    public string Name { get; } = name;

    public override void Respond(object? response)
        => Console.WriteLine($"[{Name}]: {response}");
}
