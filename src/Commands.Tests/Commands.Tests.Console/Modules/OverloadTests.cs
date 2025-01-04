namespace Commands.Tests;

[Name("overload")]
public class OverloadTests : CommandModule
{
    [Name("get")]
    public static string Get(string id = "")
    {
        return "OK!" + id;
    }

    [Name("get-other")]
    public static string Get(string id, string name)
    {
        return "OK!" + id + name;
    }
}
