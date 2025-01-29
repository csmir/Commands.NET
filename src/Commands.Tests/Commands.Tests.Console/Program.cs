using Commands;
using Commands.Testing;

var manager = ComponentCollection.With
    .Types(typeof(Program).Assembly.GetExportedTypes())
    .Handler(ResultHandler.From<ICallerContext>((c, e, s) => c.Respond(e)))
    .Component(
        Command.From((CommandContext<ConsoleContext> c) => 
        {
            foreach (var command in c.Manager!.GetCommands())
                c.Respond(command);

        }, "help"))
    .Create();

var testRunner = TestRunner.With
    .Commands(manager.GetCommands().ToArray())
    .Create();

var results = await testRunner.Run((str) => new TestContext(str));

if (results.Count(x => x.Success) == testRunner.Count)
    Console.WriteLine("All tests ran succesfully.");

while (true)
    await manager.Execute(new ConsoleContext(Console.ReadLine()));