using Commands;
using Commands.Samples;

var components = new ComponentTree
{
    new Command(() => Environment.Exit(0), "exit"),
    new CommandGroup("math")
    {
        new Command((double number, int sumBy)      
            => number + sumBy,      
                "sum", "add"),
        new Command((double number, int subtractBy) 
            => number - subtractBy, 
                "subtract", "sub"),
        new Command((double number, int multiplyBy) 
            => number * multiplyBy, 
                "multiply", "mul"),
        new Command((double number, int divideBy)   
            => number / divideBy,   
                "divide", "div")
    },
    new CommandGroup<HelpModule>()
};

var provider = new ComponentProvider(components);

provider.OnFailure += (ctx, res, ex, svc) => ctx.Respond(ex);

await provider.Execute(new ConsoleContext(args));