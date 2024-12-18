using Commands.Builders;

namespace Commands.Tests
{
    public class MutationModule : CommandModule
    {
        [Name("add-command")]
        public Task MutateCurrentModule(string commandName, [Remainder] Delegate executionAction)
        {
            var configuration = new ComponentConfigurationBuilder()
                .Build();

            var command = new CommandBuilder()
                .WithAliases(commandName)
                .WithDelegate(executionAction)
                .Build(configuration);

            Command.Parent!.Add(command);

            return Respond("Command added.");
        }

        [Name("test-delegate")]
        public Task TestDelegate([Remainder] Delegate action)
        {
            var response = action.DynamicInvoke();

            return Respond(response?.ToString() ?? "null");
        }
    }
}
