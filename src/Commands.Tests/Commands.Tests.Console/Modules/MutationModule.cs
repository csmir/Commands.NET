namespace Commands.Tests
{
    public class MutationModule : ModuleBase
    {
        [Name("add-command")]
        public Task MutateCurrentModule(string commandName)
        {
            var command = new CommandBuilder()
                .WithAliases(commandName)
                .WithDelegate(() => "Hello, World!")
                .Build(Manager.Configuration);

            Command.Module!.AddComponent(command);

            return Send("Command added.");
        }
    }
}
