using Commands;
using Commands.Testing;

var manager = ComponentManager.From()
    .Types(typeof(Program).Assembly.GetExportedTypes())
    .Handler(ResultHandler.From<ICallerContext>((c, r, s) => c.Respond(r.Unfold())))
    .Component(
        Command.From((CommandContext<ConsoleContext> c) => 
        {
            foreach (var command in c.Manager!.GetCommands())
                c.Respond(command);

        }, "help"))
    .Create();

var testRunner = TestRunner.From(manager.GetCommands().ToArray())
    .Create();

var results = await testRunner.Run((str) => new TestContext(str));

if (results.Count(x => x.Success) == testRunner.Count)
    Console.WriteLine("All tests ran succesfully.");

while (true)
    await manager.ExecuteBlocking(new ConsoleContext(Console.ReadLine()));