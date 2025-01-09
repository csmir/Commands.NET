using Commands.Testing;

namespace Commands.Tests;

[Name("class-based", "cb")]
[AND(true)]
[OR1(true)]
public class ClassModule : CommandModule
{
    [Test]
    public void Run()
    {
        Respond(Command.Attributes.Length);
        Respond("Succesfully ran " + Command.ToString());
    }
}
