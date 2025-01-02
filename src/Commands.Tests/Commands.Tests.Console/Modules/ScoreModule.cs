namespace Commands.Tests;

[Name("score")]
public class ScoreModule : CommandModule
{
    [Name("command")]
    public void ByBool(bool b)
    {
        Respond("ByBool " + b);
    }

    [Name("command")]
    public void ByInt(int i)
    {
        Respond("ByInt " + i);
    }

    public void Default()
    {
        Respond("This is a default overload");
    }

    public void Default([Remainder] string? args = null)
    {
        Respond($"This is a default overload with args: {args}");
    }
}
