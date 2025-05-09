using Commands;
using Commands.Samples;

var configuration = ComponentConfigurationBuilder.Default;

var results = ResultHandler.For<SampleContext>()
    .AddDelegate((c, e, s) => c.Respond(e));

var components = new ComponentSetBuilder()
    .WithConfiguration(configuration)
    .AddResultHandler(results)
    .Build();

while (true)
    await components.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));
