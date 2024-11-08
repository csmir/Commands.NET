﻿using Commands;
using Microsoft.Extensions.DependencyInjection;

var manager = CommandManager.CreateDefaultBuilder()
    .AddResultResolver((c, r, s) =>
    {
        if (!r.Success)
            Console.WriteLine(r);
    })
    .AddCommand("j", () => Console.WriteLine("Test"))
    .Build();

var services = new ServiceCollection();

services.AddSingleton(manager);

var provider = services.BuildServiceProvider();

//await manager.Execute(new ConsumerBase(), ["async", "true"], new()
//{
//    AsyncMode = AsyncMode.Async,
//    Services = provider.CreateAsyncScope().ServiceProvider
//});

//Console.WriteLine("Passed");

//await manager.Execute(new ConsumerBase(), ["async", "true"], new()
//{
//    AsyncMode = AsyncMode.Async,
//    Services = provider.CreateAsyncScope().ServiceProvider
//});

//Console.WriteLine("Passed");

//await manager.Execute(new ConsumerBase(), ["async", "true"], new()
//{
//    AsyncMode = AsyncMode.Async,
//    Services = provider.CreateAsyncScope().ServiceProvider
//});

//Console.WriteLine("Passed");

while (true)
{
    Console.CursorVisible = true;
    Console.Write("> ");

    using var scope = provider.CreateAsyncScope();

    await manager.Execute(new ConsumerBase(), Console.ReadLine()!, new()
    {
        AsyncMode = AsyncMode.Await,
        Services = scope.ServiceProvider
    });

    await scope.DisposeAsync();
}

//manager.Dispose();