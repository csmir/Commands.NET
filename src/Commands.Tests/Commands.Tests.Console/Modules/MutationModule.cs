using Commands.Testing;

namespace Commands.Tests;

public class MutationModule : CommandModule
{
    [Name("add-command")]
    [Test(Arguments = "test-command () => \"test\"")]
    public Task MutateCurrentModule(string commandName, [Remainder, CSharpScriptParser] Delegate executionAction)
    {
        GetParent().Add(Command.From(executionAction, commandName).ToComponent());

        return Respond("Command added.");
    }

    [Name("add-module")]
    [Test(Arguments = "test-module")]
    public Task MutateCurrentModule(string moduleName)
    {
        GetParent().Add(CommandGroup.From(moduleName).ToComponent());

        return Respond("Module added.");
    }

    [Name("add-command-to-module")]
    public Task MutateModule(string moduleName, string commandName, [Remainder, CSharpScriptParser] Delegate executionAction)
    {
        // From the parent, we can find the target module, being a CommandGroup.
        (GetParent().FirstOrDefault(x => x.Names.Contains(moduleName) && x is CommandGroup) as CommandGroup)!
            .Add(Command.From(executionAction, [commandName]).ToComponent());

        return Respond("Command added to submodule.");
    }

    private CommandGroup GetParent()
    {
        // The parent is never null in this scenario, being an instance of <MutationModule>.
        return Command.Parent!;
    }
}
