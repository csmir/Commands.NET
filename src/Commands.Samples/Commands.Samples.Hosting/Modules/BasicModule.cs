namespace Commands.Samples;

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
