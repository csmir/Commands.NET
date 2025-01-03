namespace Commands.Tests;

[Name("module")]
public class Module : CommandModule
{
    [Name("command1")]
    public static string ACommand()
        => "Test";

    [Name("command2")]
    public static void ACommand(params int[] arg)
        => Console.WriteLine(string.Join(", ", arg));

    [Name("command3")]
    public static Task<string> Respond(string str)
        => Task.FromResult(str);

    public class CallerContext : ICallerContext
    {
        public void Respond(object? response)
            => Console.WriteLine(response);
    }
}
