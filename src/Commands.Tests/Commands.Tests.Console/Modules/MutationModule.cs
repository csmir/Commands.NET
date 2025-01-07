namespace Commands.Tests;

public class MutationModule : CommandModule
{
    [Name("add-command")]
    [TryInput("test-command () => { }")]
    public Task MutateCurrentModule(string commandName, [Remainder, CSharpScriptParser] Delegate executionAction)
    {
        GetParent().Add(Command.Create(executionAction, commandName));

        return Respond("Command added.");
    }

    [Name("add-module")]
    [TryInput("test-module")]
    public Task MutateCurrentModule(string moduleName)
    {
        GetParent().Add(CommandGroup.Create(moduleName));

        return Respond("Module added.");
    }

    [Name("add-command-to-module")]
    public Task MutateModule(string moduleName, string commandName, [Remainder, CSharpScriptParser] Delegate executionAction)
    {
        // From the parent, we can find the target module, being a CommandGroup.
        (GetParent().FirstOrDefault(x => x.Names.Contains(moduleName) && x is CommandGroup) as CommandGroup)!
            .Add(Command.Create(executionAction, [commandName]));

        return Respond("Command added to submodule.");
    }

    private CommandGroup GetParent()
    {
        // The parent is never null in this scenario, being an instance of <MutationModule>.
        return Command.Parent!;
    }
}
