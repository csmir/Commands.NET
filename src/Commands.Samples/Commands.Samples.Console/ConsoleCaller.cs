
namespace Commands.Samples;

public class ConsoleCaller(string name) : ICallerContext
{
    public string Name { get; } = name;

    public void Respond(object? response)
        => Console.WriteLine($"[{Name}] {response}");
}
