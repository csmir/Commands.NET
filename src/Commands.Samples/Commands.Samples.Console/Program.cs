using Commands;
using Commands.Samples;

var components = new ComponentTree();

components.AddRange(typeof(Program).Assembly.GetExportedTypes());

var provider = new ComponentProvider(components);

provider.OnFailure += (ctx, res, ex, srv) => ctx.Respond(ex);

while (true)
    await provider.Execute(new SampleContext(username: "Peter", args: Console.ReadLine()));
