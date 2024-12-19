using Commands;
using Commands.Tests;
using Microsoft.Extensions.DependencyInjection;

var tree = ComponentTree.CreateBuilder()
    .ConfigureComponents(c =>
    {
        c.AddParser(new CSharpScriptParser());
    })
    .AddResultHandler((c, r, s) =>
    {
        c.Respond(r);
    })
    .AddModule(m =>
    {
        m.WithAliases("level1");
        m.AddCommand("a", () => Console.WriteLine("Test"));
        m.AddModule(s =>
        {
            s.WithAliases("level2");
            s.AddCommand("b", () => Console.WriteLine("Test"));
            s.AddCommand(() => Console.WriteLine("Test"));
        });
    })
    .AddCommand("j", () => Console.WriteLine("Test"))
    .Build();

var services = new ServiceCollection();

services.AddSingleton(tree);

var provider = services.BuildServiceProvider();

while (true)
{
    Console.CursorVisible = true;
    Console.Write("> ");

    using var scope = provider.CreateAsyncScope();

    await tree.Execute(new CustomCaller(), Console.ReadLine()!, new()
    {
        Services = scope.ServiceProvider
    });

    await scope.DisposeAsync();
}