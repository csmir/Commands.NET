
namespace Commands.Samples;

public class CustomCaller(string name) : ICallerContext
{
    public string Name { get; } = name;

    public void Respond(object? response)
        => System.Console.WriteLine($"[{Name}] {response}");
}
