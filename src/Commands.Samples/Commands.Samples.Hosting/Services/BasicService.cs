using Commands.Hosting;

namespace Commands.Samples;

// This class represents a basic service that provides functionality to get and set the version of the application,
// as well as to retrieve a list of available commands. We abstract this functionality into a service to keep the command logic clean and focused on execution patterns.
public class BasicService(IContextAccessor<ConsoleContext> context, IComponentProvider provider, VersionManager manager)
{
    public IEnumerable<string> GetCommands()
    {
        var commands = provider.Components.GetCommands(true)
            .Select(command => command.ToString())
            .ToArray();

        // If no commands were found, return a message indicating that, instead of returning just an empty list.
        if (commands.Length == 0)
            context.Context.Respond("No commands found.");

        return commands;
    }

    public Version GetVersion()
    {
        // Use the VersionManager to get the current version of the application.
        return manager.CurrentVersion;
    }

    public void SetVersion(Version version)
    {
        if (version < manager.CurrentVersion)
            context.Context.Respond("Cannot set version to a previous version.");

        else if (version == manager.CurrentVersion)
            context.Context.Respond("Version is already set to the specified version.");

        else
        {
            manager.CurrentVersion = version;

            context.Context.Respond($"Version set to {manager.CurrentVersion}.");
        }
    }
}
