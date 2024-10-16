namespace Commands.Samples
{
    // Commands can also be sent from static classes and static methods. The prerequisite for this, is that the static method must be parameterized with a CommandContext, just like below.
    public static class StaticCommands
    {
        [Name("static")]
        [Description("Sends a hello world message.")]
        public static void StaticHelloWorld(CommandContext<ConsumerBase> context)
        {
            // The CommandContext contains the options used to execute this command, the command information, and the consumer that the command was executed by.

            context.Consumer.SendAsync("Hello world!");
        }
    }
}
