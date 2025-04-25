using Commands;
using Commands.Samples;
using Commands.Testing;

var configuration = ComponentConfigurationProperties.Default;

var results = ResultHandler.For<SampleContext>()
    .AddDelegate((c, e, s) => c.Respond(e));

var components = new ComponentCollectionProperties()
    .WithConfiguration(configuration)
    .AddResultHandler(results)
    .ToCollection();

var tests = new TestCollectionProperties()
    .AddCommands([.. components.GetCommands()])
    .ToCollection();

var testEvaluation = await tests.Execute((str) => new TestContext(str));

if (testEvaluation.Count(x => x.Success) == tests.Count)
    Console.WriteLine("All tests ran successfully.");

while (true)
    await components.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));