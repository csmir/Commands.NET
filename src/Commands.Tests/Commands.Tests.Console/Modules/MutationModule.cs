namespace Commands.Tests
{
    public class MutationModule : CommandModule
    {
        [Name("add-command")]
        public Task MutateCurrentModule(string commandName, [Remainder] Delegate executionAction)
        {
            var command = new CommandBuilder()
                .WithAliases(commandName)
                .WithDelegate(executionAction)
                .Build([]);

            Command.Parent!.Add(command);

            return Send("Command added.");
        }
    }
}
