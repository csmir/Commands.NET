using Commands;
using Commands.Samples;

var provider = ComponentProvider.CreateBuilder()
    .AddComponentTypes(typeof(Program).Assembly.GetExportedTypes())
    .AddResultHandler<SampleContext>((c, e, s) => c.Respond(e))
    .Build();

while (true)
    await provider.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));
