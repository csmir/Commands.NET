namespace Commands.Samples;

[Name("class")]
public sealed class ClassModule : CommandModule
{
    public string ClassCommand()
    {
        return "Hello from a class command!";
    }

    [Ignore]
    public void NotACommand()
    {

    }
}
