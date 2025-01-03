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
            var description = command.Attributes.OfType<DescriptionAttribute>().FirstOrDefault()?.Description ?? "No description available.";

            await Respond(command.ToString() ?? "Unnamed component.");
            await Respond(command.FullName ?? "Unnamed command.");
            await Respond(description);
        }
    }
}
