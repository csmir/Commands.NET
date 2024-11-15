namespace Commands.Tests
{
    public sealed class HelpModule : ModuleBase
    {
        [Name("help")]
        public void Help()
        {
            var commands = Manager.GetCommands();
            foreach (var command in commands)
            {
                Send(command.FullName);
            }
        }
    }
}
