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
                .Build(Manager.Configuration);

            Command.Module!.AddComponent(command);

            return Send("Command added.");
        }
    }
}
