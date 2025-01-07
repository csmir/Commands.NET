namespace Commands.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class TryInputAttribute(string input, bool shouldFail = false) : Attribute
{
    public string Input { get; } = input;

    public bool ShouldFail { get; } = shouldFail;
}
