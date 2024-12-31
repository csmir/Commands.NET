using Commands;
using Commands.Tests;

var tree = ComponentTree.CreateBuilder()
    .AddType<Module>()
    .AddResultHandler((c, res, serv) =>
    {
        c.Respond(res);

        return ValueTask.CompletedTask;
    })
    .Build();

while (true)
{
    await tree.Execute(new Module.CallerContext(), Console.ReadLine()!);
}