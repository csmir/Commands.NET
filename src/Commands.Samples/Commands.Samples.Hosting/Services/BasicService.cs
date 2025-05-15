using Commands.Hosting;

namespace Commands.Samples;

public class BasicService(IContextAccessor<ConsoleCallerContext> caller, IComponentProvider provider)
{
    public IEnumerable<string> GetCommands()
    {
        var commands = provider.Components.GetCommands(true)
            .Select(command => command.ToString())
            .ToArray();

        // If no commands were found, return a message indicating that, instead of returning just an empty list.
        if (commands.Length == 0)
            caller.Caller.Respond("No commands found.");

        return commands;
    }
}
