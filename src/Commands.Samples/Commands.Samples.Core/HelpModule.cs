using System.Text;

namespace Commands.Samples;

public class HelpModule(IComponentProvider provider) : CommandModule
{
    [Name("help")]
    public string Help()
    {
        var builder = new StringBuilder()
            .AppendLine("Commands:");

        foreach (var command in provider.Components.GetCommands())
            builder.AppendLine(command.GetFullName());

        return builder.ToString();
    }

    [Name("help")]
    public string Help([Remainder, Name("command-name")] string commandName)
    {
        var commands = provider.Components.GetCommands();

        var command = commands
            .Where(x => x.GetFullName().StartsWith(commandName))
            .ToArray();

        if (command.Length == 0)
            return "Command not found.";

        if (command.Length > 1)
        {
            var builder = new StringBuilder()
                .AppendLine("Multiple commands found:");

            for (var i = 0; i < command.Length; i++)
                builder.AppendLine(command[i].GetFullName());

            return builder.ToString();
        }

        return command[0].ToString();
    }
}
