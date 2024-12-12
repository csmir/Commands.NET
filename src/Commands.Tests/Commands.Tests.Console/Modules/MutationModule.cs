namespace Commands.Tests
{
    public class MutationModule : ModuleBase
    {
        [Name("add-command")]
        public Task MutateCurrentModule(string commandName, [Remainder] Delegate executionAction)
        {
            var command = new CommandBuilder()
                .WithAliases(commandName)
                .WithDelegate(executionAction)
                .Build([]);

            Command.Module!.Add(command);

            return Send("Command added.");
        }
    }
}
