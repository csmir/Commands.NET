using System.ComponentModel;

namespace Commands.Samples
{
    public class StaticCommands : CommandModule
    {
        [Name("static")]
        [Description("Sends a hello world message.")]
        public static void StaticHelloWorld(CommandContext<CustomConsumer> context)
        {
            context.Caller.Respond("Hello world!");
        }
    }
}
