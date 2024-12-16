namespace Commands.Samples
{
    public sealed class Module : CommandModule<CallerContext>
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
