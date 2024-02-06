
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Commands.Parsing;

[MemoryDiagnoser]
public class Program
{
    private static readonly StringParser _parser = new();

    [Params("command", "a larger command with context", "a massive command \"with quotes\" and several additional 1 22 333 4444 5555")]
    public string? Text { get; set; }

    static void Main()
        => BenchmarkRunner.Run<Program>();

    [Benchmark]
    public void ParseText()
    {
        _parser.Parse(Text);
    }
}