using System.ComponentModel;

namespace Commands.Samples;

public sealed class HelpModule(IComponentProvider provider) : CommandModule
{
    [Name("help")]
    public async Task Help()
    {
        var commands = provider.Components.GetCommands();

        foreach (var command in commands)
        {
            var description = command.Attributes.FirstOrDefault<DescriptionAttribute>()?.Description ?? "No description available.";

            await Respond(command.GetFullName());
            await Respond(description);
        }
    }
}
