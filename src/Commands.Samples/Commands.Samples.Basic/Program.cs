﻿using Commands;
using Commands.Console;
using Spectre.Console;

var manager = CommandManager.CreateBuilder()
    .AddCommand("command", () =>
    {
        return "This is my first command!";
    })
    .AddCommand("help", (CommandContext<ConsoleConsumerBase> ctx) =>
    {
        var commands = ctx.Manager.GetCommands();

        foreach (var command in commands)
        {
            var description = command.GetAttribute<DescriptionAttribute>()?.Description ?? "No description available.";

            ctx.Send($"[yellow]{command.ToString().EscapeMarkup()}[/]");
            ctx.Send($"[blue]{description}[/]");
        }
    })
    .AddResultResolver((consumer, result, services) =>
    {
        if (!result.Success)
        {
            consumer.Send(result.Exception!);
        }
    })
    .Build();

while (true)
{
    await manager.Execute(new ConsoleConsumerBase(), Console.ReadLine()!);
}