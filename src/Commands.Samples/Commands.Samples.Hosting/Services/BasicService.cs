using Commands.Hosting;

namespace Commands.Samples;

public class BasicService(ICallerContextAccessor<ConsoleCallerContext> caller, IExecutionProvider provider)
{
    public List<string> GetCommands()
    {
        var commands = new List<string>();
        foreach (var command in provider.GetCommands())
        {
            commands.Add(command.ToString());
        }

        // If no commands were found, return a message indicating that, instead of returning just an empty list.
        if (commands.Count == 0)
            caller.Caller.Respond("No commands found.");

        return commands;
    }
}
