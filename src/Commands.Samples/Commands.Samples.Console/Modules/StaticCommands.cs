﻿using System.ComponentModel;

namespace Commands.Samples;

public class StaticCommands : CommandModule
{
    [Name("static")]
    [Description("Sends a hello world message.")]
    public static void StaticHelloWorld(CommandContext<ConsoleCaller> context)
    {
        context.Caller.Respond("Hello world!");
    }
}
