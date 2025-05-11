using Commands;
using Commands.Samples;

var components = new ComponentTree();

components.AddRange(typeof(Program).Assembly.GetExportedTypes());

var provider = new ComponentProvider(components, new DelegateResultHandler<SampleContext>((c, e, s) => Console.WriteLine(e)));

while (true)
    await provider.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));
