namespace Commands.Tests;

public class MutationModule : CommandModule
{
    [Name("add-command")]
    public Task MutateCurrentModule(string commandName, [Remainder] Delegate executionAction)
    {
        Command.Parent!.Add(Commands.Command.Create(executionAction, [commandName]));

        return Respond("Command added.");
    }

    [Name("test-delegate")]
    public Task TestDelegate([Remainder] Delegate action)
    {
        var response = action.DynamicInvoke();

        return Respond(response?.ToString() ?? "null");
    }
}
