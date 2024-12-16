namespace Commands.Tests
{
    public class MutationModule : CommandModule
    {
        [Name("add-command")]
        public Task MutateCurrentModule(string commandName, [Remainder] Delegate executionAction)
        {
            var configuration = new BuildConfigurationBuilder()
                .Build();

            var command = new CommandBuilder()
                .WithAliases(commandName)
                .WithDelegate(executionAction)
                .Build(configuration);

            Command.Parent!.Add(command);

            return Send("Command added.");
        }
    }
}
