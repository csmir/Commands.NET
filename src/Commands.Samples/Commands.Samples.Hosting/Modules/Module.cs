namespace Commands.Samples
{
    public sealed class Module : CommandModule
    {
        [Name("help")]
        public void Help()
        {
            var commands = Tree.GetCommands();

            foreach (var command in commands)
            {
                Respond(command.FullName ?? "Unnamed command.");
            }
        }
    }
}
