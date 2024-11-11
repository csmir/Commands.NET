namespace Commands.Samples
{
    public class HelpModule : ModuleBase
    {
        [Name("help")]
        public async Task Help()
        {
            var commands = Manager.GetCommands();

            foreach (var command in commands)
            {
                var description = command.GetAttribute<DescriptionAttribute>()?.Description ?? "No description available.";

                await Consumer.Send(command.ToString());
                await Consumer.Send(description);
            }
        }
    }
}
