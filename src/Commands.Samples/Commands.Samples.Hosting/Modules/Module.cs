namespace Commands.Samples;

// By defining a module, we can create commands.
public sealed class Module : CommandModule<HostedCallerContext>
{
    [Name("help")]
    public void Help()
    {
        foreach (var command in Manager!.GetCommands())
            Respond(command);
    }

    [Name("version")]
    public Version AssemblyVersion()
        => typeof(Module).Assembly.GetName().Version ?? new(1, 0);

    [Name("echo")]
    public string Echo(string message)
        => message;
}
