namespace Commands.Tests;

public class AutomatedContext(string input, bool shouldFail) : ICallerContext
{
    public string Input { get; } = input;

    public bool ShouldFail { get; } = shouldFail;

    public void Respond(object? message) { } // Do not send anything.
}
