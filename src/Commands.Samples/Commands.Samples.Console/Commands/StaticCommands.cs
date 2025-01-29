using System.ComponentModel;

namespace Commands.Samples;

public sealed class StaticCommands : CommandModule
{
    [Name("static")]
    [Description("Sends a hello world message.")]
    public static void StaticHelloWorld(CommandContext<SampleContext> context)
    {
        context.Caller.Respond("Hello world!");
    }
}
