using Commands.Hosting;

namespace Commands.Samples;

public class BasicService(ICallerContextAccessor<ConsoleCallerContext> caller, IExecutionProvider provider)
{
    public IEnumerable<string> GetCommands()
    {
        var commands = provider.GetCommands(true)
            .Select(command => command.ToString())
            .ToArray();

        // If no commands were found, return a message indicating that, instead of returning just an empty list.
        if (commands.Length == 0)
            caller.Caller.Respond("No commands found.");

        return commands;
    }
}
