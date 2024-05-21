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

while (true)
{
    var input = StringParser.Parse(Console.ReadLine());

    await manager.TryExecuteAsync(new ConsumerBase(), input, new()
    {
        Services = provider.CreateAsyncScope().ServiceProvider
    });
}