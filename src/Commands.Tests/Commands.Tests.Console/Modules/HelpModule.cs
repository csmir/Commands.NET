namespace Commands.Tests;

public sealed class HelpModule : CommandModule
{
    [Name("help")]
    public void Help()
    {
        var commands = Tree.GetCommands();
        foreach (var command in commands)
        {
            command.GetAttribute<ANDConditionAttribute>();

            Respond(command.FullName);
        }
    }
}
