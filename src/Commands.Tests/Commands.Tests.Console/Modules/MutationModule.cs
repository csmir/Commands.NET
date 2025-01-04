namespace Commands.Tests;

public class MutationModule : CommandModule
{
    [Name("add-command")]
    public Task MutateCurrentModule(string commandName, [Remainder] Delegate executionAction)
    {
        // The parent is never null, being an instance of <MutationModule>.
        // We can safely access it without null-checking.
        Command.Parent!.Add(Commands.Command.Create(executionAction, commandName));

        return Respond("Command added.");
    }

    [Name("add-module")]
    public Task MutateCurrentModule(string moduleName)
    {
        Command.Parent!.Add(CommandGroup.Create(moduleName));

        return Respond("Module added.");
    }

    [Name("add-command-to-module")]
    public Task MutateModule(string moduleName, string commandName, [Remainder] Delegate executionAction)
    {
        // From the parent, we can find the target module, being a CommandGroup.
        (Command.Parent!.FirstOrDefault(x => x.Names.Contains(moduleName) && x is CommandGroup) as CommandGroup)!
            .Add(Commands.Command.Create(executionAction, [commandName]));

        return Respond("Command added to submodule.");
    }
}
