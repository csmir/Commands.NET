using System.ComponentModel;

namespace Commands.Samples;

public class HelpModule : CommandModule
{
    [Name("help")]
    public async Task Help()
    {
        var commands = Tree.GetCommands();

        foreach (var command in commands)
        {
            var description = command.GetAttribute<DescriptionAttribute>()?.Description ?? "No description available.";

            await Respond(command.GetFullName());
            await Respond(description);
        }
    }
}
