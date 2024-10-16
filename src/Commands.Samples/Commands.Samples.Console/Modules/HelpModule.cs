namespace Commands.Samples
{
    // Commands have access to the CommandManager through the ModuleBase in which they are written, which can be used to retrieve all commands, or all modules.
    public class HelpModule : ModuleBase
    {
        [Name("help")]
        public async Task Help()
        {
            var commands = Manager.GetCommands();

            foreach (var command in commands)
            {
                // Commands contain a method called GetAttribute to retrieve specific attributes, such as the DescriptionAttribute, which can be used to provide a description of the command.

                var description = command.GetAttribute<DescriptionAttribute>()?.Description ?? "No description available.";

                await Consumer.SendAsync(command.ToString());
                await Consumer.SendAsync(description);
            }
        }
    }
}
