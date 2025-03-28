using Commands;
using Commands.Samples;
using Commands.Testing;

var configuration = ComponentConfigurationProperties.Default;

var results = ResultHandler.For<SampleContext>()
    .Delegate((c, e, s) => c.Respond(e));

var manager = ComponentCollection.With
    .Configuration(configuration)
    .Handler(results)
    .Create();

var testRunner = TestRunner.With
    .Commands([.. manager.GetCommands()])
    .Create();

var testEvaluation = await testRunner.Run((str) => new TestContext(str));

if (testEvaluation.Count(x => x.Success) == testRunner.Count)
    Console.WriteLine("All tests ran successfully.");

while (true)
    await manager.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));