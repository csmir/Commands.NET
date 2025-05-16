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
            caller.Context.Respond("No commands found.");

        caller.Context.Respond("Commands found:");

        return commands;
    }
}
