using Commands;
using Commands.Tests;

var tree = ComponentTree.CreateBuilder()
    .AddType<Module>()
    .AddResultHandler(async (c, res, serv) => await c.Respond(res))
    .Build();

while (true)
    await tree.Execute(new Module.CallerContext(), Console.ReadLine()!);