﻿using Commands;
using Microsoft.Extensions.DependencyInjection;

var manager = CommandManager.CreateDefaultBuilder()
    .AddResultResolver((c, r, s) =>
    {
        if (!r.Success)
            Console.WriteLine(r);
    })
    .AddModule(module =>
    {
        module.WithAliases("level1");
        module.AddCommand("a", () => Console.WriteLine("Test"));
        module.AddModule(submodule =>
        {
            submodule.WithAliases("level2");
            submodule.AddCommand("b", () => Console.WriteLine("Test"));
            submodule.AddCommand(() => Console.WriteLine("Test"));
        });
    })
    .AddCommand("j", () => Console.WriteLine("Test"))
    .Build();

var services = new ServiceCollection();

services.AddSingleton(manager);

var provider = services.BuildServiceProvider();

while (true)
{
    Console.CursorVisible = true;
    Console.Write("> ");

    using var scope = provider.CreateAsyncScope();

    await manager.Execute(new ConsumerBase(), Console.ReadLine()!, new()
    {
        Services = scope.ServiceProvider
    });

    await scope.DisposeAsync();
}