namespace Commands.Samples;

[Name("class")]
public class ClassModule : CommandModule
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
