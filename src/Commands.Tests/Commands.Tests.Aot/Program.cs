using Commands;
using Commands.Tests;

var manager = ComponentManager.Define()
    .Type<Module>()
    .Handler(ResultHandler.Define<ICallerContext>((c, res, serv) => c.Respond(res)))
    .ToManager();

while (true)
    manager.TryExecute(new Module.CallerContext(), Console.ReadLine()!);