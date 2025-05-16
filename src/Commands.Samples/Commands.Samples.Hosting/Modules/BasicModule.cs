namespace Commands.Samples;

public class BasicModule(BasicService service) : CommandModule<ConsoleContext>
{
    [Name("help")]
    public IEnumerable<string> Help()
        => service.GetCommands();

    [Name("version")]
    public Version AssemblyVersion()
        => typeof(BasicModule).Assembly.GetName().Version ?? new(1, 0);

    [Name("echo")]
    public string Echo(string message)
        => message;
}
