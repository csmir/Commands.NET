using Commands.Testing;

namespace Commands.Tests;

[Name("score")]
public class ScoreModule : CommandModule
{
    [Name("command")]
    [Test(Arguments = "true")]
    public void ByBool(bool b)
    {
        Respond("ByBool " + b);
    }

    [Name("command")]
    [Test(Arguments = "1")]
    public void ByInt(int i)
    {
        Respond("ByInt " + i);
    }

    // Prioritize the default overload if no arguments are provided, for it will otherwise prioritize the overload with (optional) arguments.
    [Priority(5)]
    [Test]
    public void Default()
    {
        Respond("This is a default overload");
    }

    [Priority(1)]
    [Test(Arguments = "args args args args")]
    public void Default([Remainder] string? args = null)
    {
        Respond($"This is a default overload with args: {args}");
    }
}
