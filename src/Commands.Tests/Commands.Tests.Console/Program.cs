using Commands;
using Commands.Testing;

var components = new ComponentCollectionProperties()
    .AddTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddHandler(ResultHandler.From<ICallerContext>((c, e, s) => c.Respond(e)))
    .AddComponent(
        Command.From((CommandContext<ConsoleContext> c) => 
        {
            foreach (var command in c.Manager!.GetCommands())
                c.Respond(command);

        }, "help"))
    .Create();

var tests = TestCollection.From([.. components.GetCommands()])
    .Create();

var results = await tests.Execute((str) => new TestContext(str));

if (results.Count(x => x.Success) == tests.Count)
    Console.WriteLine("All tests ran succesfully.");

while (true)
    await components.Execute(new ConsoleContext(Console.ReadLine()));