namespace Commands.Samples;

public sealed class AsyncModule : CommandModule
{
    [Name("task-propagate")]
    public Task Propagate()
        => Respond("Hello from a task command.");

    [Name("task-await")]
    public async Task Await()
        => await Respond("Hello from a task command with await.");

    [Name("task-return")]
    public Task<string> Return()
        => Task.FromResult("Hello from a task command.");
}
