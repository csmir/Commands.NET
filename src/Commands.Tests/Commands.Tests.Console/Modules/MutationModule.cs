namespace Commands.Tests
{
    public class MutationModule : ModuleBase
    {
        [Name("add-command")]
        public async Task MutateCurrentModule(string commandName)
        {
            var command = new CommandBuilder();

            command.WithAliases(commandName);
            command.WithDelegate(() => "Hello, World!");

            Command.Module!.Components.Add(command.Build(Manager.Configuration));
        }
    }
}
