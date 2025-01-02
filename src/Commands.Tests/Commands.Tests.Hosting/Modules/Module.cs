namespace Commands.Tests;

public sealed class Module : CommandModule
{
    [Name("test")]
    public void Test()
    {
        Console.WriteLine("Tested");
    }
}
