namespace Commands.Tests
{
    public class MutationModule : ModuleBase
    {
        [Name("add-command")]
        public async Task MutateCurrentModule(string commandName)
        {
            await Task.CompletedTask;

            var command = new CommandBuilder();

            command.WithAliases(commandName);
            command.WithDelegate(() => "Hello, World!");

            var result = command.Build(Manager.Configuration);

            Command.Module!.Components.Add(result);
            Command.Module.Sort();
        }
    }
}
