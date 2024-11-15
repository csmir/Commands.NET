namespace Commands.Samples
{
    public sealed class Module : ModuleBase<ConsumerBase>
    {
        [Name("help")]
        public void Help()
        {
            var commands = Manager.GetCommands();

            foreach (var command in commands)
            {
                Send(command.FullName ?? "Unnamed command.");
            }
        }
    }
}
