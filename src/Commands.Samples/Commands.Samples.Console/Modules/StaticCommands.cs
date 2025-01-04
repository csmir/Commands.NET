using System.ComponentModel;

namespace Commands.Samples;

public class StaticCommands : CommandModule
{
    [Name("static")]
    [Description("Sends a hello world message.")]
    [RequireContext<ConsoleCallerContext>]
    public static void StaticHelloWorld(CommandContext<ConsoleCallerContext> context)
    {
        context.Caller.Respond("Hello world!");
    }
}
