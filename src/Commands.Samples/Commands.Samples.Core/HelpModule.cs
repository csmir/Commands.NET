using System.Text;

namespace Commands.Samples;

[Name("help")]
public class HelpModule : CommandModule
{
    public void Help(int count = 10)
    {
        // Ignore overloads and just list commands by name.
        var commands = Tree.GetCommands().ToArray();

        var builder = new StringBuilder();

        builder.AppendLine("Commands:");

        for (var i = 0; i < count && i < (commands.Length > count ? count : commands.Length); i++)
            builder.AppendLine(commands[i].GetFullName());

        Respond(builder.ToString());
    }

    public void Help([Remainder] string commandName)
    {
        var commands = Tree.GetCommands();

        var command = commands.FirstOrDefault(x => x.GetFullName().StartsWith(commandName));

        if (command is null)
        {
            Respond("Command not found.");
            return;
        }

        Respond(command);
    }
}
