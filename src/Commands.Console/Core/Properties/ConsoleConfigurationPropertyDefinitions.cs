using System.ComponentModel;

namespace Commands;

/// <summary>
///     Defines the names of the configuration properties that are used to configure a console Commands.NET environment.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public static class ConsoleConfigurationPropertyDefinitions
{
    /// <summary>
    ///     Gets the name of the configuration property that defines the default command name for CLI applications.
    /// </summary>
    public const string CLIDefaultOverloadName = "CLIDefaultOverloadName";
}
