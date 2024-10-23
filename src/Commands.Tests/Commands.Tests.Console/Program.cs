using Commands;
using Commands.Parsing;
using Microsoft.Extensions.DependencyInjection;

var manager = CommandManager.CreateBuilder()
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

    var input = StringParser.Parse(Console.ReadLine());

    //var input = new Dictionary<string, object?>()
    //{
    //    ["command"] = null,
    //    ["nested"] = null,
    //    ["complex"] = null,
    //    ["2"] = null,
    //    ["3"] = null,
    //    ["xx"] = "1",
    //    ["x"] = "2",
    //};

    using var scope = provider.CreateAsyncScope();

    await manager.Execute(new ConsumerBase(), input, new()
    {
        AsyncMode = AsyncMode.Await,
        Services = scope.ServiceProvider
    });

    await scope.DisposeAsync();
}

//manager.Dispose();