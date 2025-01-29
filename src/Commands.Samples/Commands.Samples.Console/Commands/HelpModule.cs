using System.ComponentModel;

namespace Commands.Samples;

public sealed class HelpModule : CommandModule
{
    [Name("help")]
    public async Task Help()
    {
        var commands = Manager!.GetCommands();

        foreach (var command in commands)
        {
            var description = command.Attributes.FirstOrDefault<DescriptionAttribute>()?.Description ?? "No description available.";

            await Respond(command.GetFullName());
            await Respond(description);
        }
    }
}
