using Commands;
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