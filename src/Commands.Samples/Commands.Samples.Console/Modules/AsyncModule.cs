namespace Commands.Samples;

public class AsyncModule : CommandModule
{
    [Name("task-string")]
    public Task<string> GetString()
    {
        return Task.FromResult("Hello from a task command.");
    }

    [Name("task-empty")]
    public Task GetEmpty()
    {
        return Respond("Hello from a task command with no return value.");
    }
}
