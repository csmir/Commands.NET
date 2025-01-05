namespace Commands.Samples;

public sealed class Module : CommandModule
{
    [Name("help")]
    public void Help()
    {
        foreach (var command in Manager!.GetCommands())
            Respond(command);
    }
}
