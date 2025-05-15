using System.ComponentModel;

namespace Commands.Samples;

public sealed class StaticCommands : CommandModule
{
    [Name("static")]
    [Description("Sends a hello world message.")]
    public static void StaticHelloWorld(ICallerContext context)
    {
        context.Respond("Hello world!");
    }
}
