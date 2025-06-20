using System.ComponentModel;

namespace Commands.Samples;

public sealed class HelpModule(IComponentProvider provider) : CommandModule
{
    [Name("help")]
    public void Help()
    {
        var commands = provider.Components.GetCommands();

        foreach (var command in commands)
        {
            var description = command.Attributes.OfType<DescriptionAttribute>().FirstOrDefault()?.Description ?? "No description available.";

            Respond(command.GetFullName());
            Respond(description);
        }
    }
}
