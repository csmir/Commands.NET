namespace Commands.Samples;

// This class represents a basic command module that provides commands for getting help, checking the version, and echoing messages.
public class BasicModule(BasicService service) : CommandModule<ConsoleContext>
{
    [Name("help")]
    public IEnumerable<string> Help()
        => service.GetCommands();

    [Name("version")]
    public Version Version()
        => service.GetVersion();

    [Name("version")]
    public void SetVersion(Version version)
        => service.SetVersion(version);

    [Name("echo")]
    public string Echo(string message)
        => message;
}
