namespace Commands.Samples
{
    public class HelpModule : ModuleBase
    {
        private readonly CommandManager _manager;

        public HelpModule(CommandManager manager) 
        {
            _manager = manager;
        }

        public async Task Help()
        {
            var commands = _manager.FlattenCommands();

            foreach (var command in commands)
            {
                await Consumer!.SendAsync($"{command.Name}: [{string.Join(", ", command.Aliases)}] - {command.Attributes.Where(x => x is DescriptionAttribute).Select(x => (x as DescriptionAttribute)!.Description).FirstOrDefault()}");
            }
        }
    }
}
