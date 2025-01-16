using System.Text;

namespace Commands.Samples;

public class HelpModule : CommandModule
{
    [Name("help")]
    public void Help()
    {
        var builder = new StringBuilder()
            .AppendLine("Commands:");

        foreach (var command in Manager!.GetCommands())
            builder.AppendLine(command.GetFullName());

        Respond(builder.ToString());
    }

    [Name("help")]
    public void Help([Remainder, Name("command-name")] string commandName)
    {
        var commands = Manager!.GetCommands();

        var command = commands
            .Where(x => x.GetFullName().StartsWith(commandName))
            .ToArray();

        if (command.Length == 0)
        {
            Respond("Command not found.");
            return;
        }

        if (command.Length > 1)
        {
            var builder = new StringBuilder()
                .AppendLine("Multiple commands found:");

            for (var i = 0; i < command.Length; i++)
                builder.AppendLine(command[i].GetFullName());

            Respond(builder.ToString());

            return;
        }

        Respond(command[0]);
    }
}
