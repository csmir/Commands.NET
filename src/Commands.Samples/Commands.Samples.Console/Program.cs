using Commands;
using Commands.Samples;

var components = new ComponentTree();

components.AddRange(typeof(Program).Assembly.GetExportedTypes());

var provider = new ComponentProvider(components, new HandlerDelegate<SampleContext>((c, e, s) => c.Respond(e)));

while (true)
    await provider.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));
