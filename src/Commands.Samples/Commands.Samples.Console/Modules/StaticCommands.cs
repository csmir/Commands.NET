namespace Commands.Samples
{
    public class StaticCommands : ModuleBase
    {
        [Name("static")]
        [Description("Sends a hello world message.")]
        public static void StaticHelloWorld(CommandContext<CustomConsumer> context)
        {
            context.Consumer.Send("Hello world!");
        }
    }
}
