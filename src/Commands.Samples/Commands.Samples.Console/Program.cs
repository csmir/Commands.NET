using Commands;
using Commands.Samples;
using Commands.Testing;

var configuration = ComponentConfigurationProperties.Default;

var results = ResultHandler.For<SampleContext>()
    .AddDelegate((c, e, s) => c.Respond(e));

var components = new ComponentProviderProperties()
    .WithConfiguration(configuration)
    .AddResultHandler(results)
    .ToProvider();

var tests = components.GetCommands().Select(x => TestProvider.From(x).ToProvider());

foreach (var test in tests)
{
    var result = await test.Test(x => new ConsoleCallerContext(x));

    if (result.Any(x => !x.Success))
        throw new InvalidOperationException($"A command test failed to evaluate to success. Command: {test.Command}. Test: {result.FirstOrDefault(x => !x.Success).Test}");
}

while (true)
    await components.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));
