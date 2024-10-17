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

await manager.Execute(new ConsumerBase(), ["async", "true"], new()
{
    AsyncMode = AsyncMode.Async,
    Services = provider.CreateAsyncScope().ServiceProvider
});

Console.WriteLine("Passed");

await manager.Execute(new ConsumerBase(), ["async", "true"], new()
{
    AsyncMode = AsyncMode.Async,
    Services = provider.CreateAsyncScope().ServiceProvider
});

Console.WriteLine("Passed");

await manager.Execute(new ConsumerBase(), ["async", "true"], new()
{
    AsyncMode = AsyncMode.Async,
    Services = provider.CreateAsyncScope().ServiceProvider
});

Console.WriteLine("Passed");

while (true)
{
    var input = StringParser.Parse(Console.ReadLine());

    await manager.Execute(new ConsumerBase(), input, new()
    {
        AsyncMode = AsyncMode.Async,
        Services = provider.CreateAsyncScope().ServiceProvider
    });
}

//manager.Dispose();