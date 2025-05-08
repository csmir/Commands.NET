
using Commands;
using Commands.Samples;

var exit = Command.From("exit")
    .AddDelegate(() => Environment.Exit(0));

var mathCommands = CommandGroup.From("math")
    .AddComponents(
        Command.From(Sum, "sum", "add"),
        Command.From(Subtract, "subtract", "sub"),
        Command.From(Multiply, "multiply", "mul"),
        Command.From(Divide, "divide", "div")
    );

var components = ExecutableComponentSet.From(exit, mathCommands)
    .AddComponentType<HelpModule>()
    .Build();

await components.Execute(new ConsoleCallerContext(args));

static double Sum(double number, int sumBy)
    => number + sumBy;
static double Subtract(double number, int subtractBy)
    => number - subtractBy;
static double Multiply(double number, int multiplyBy)
    => number * multiplyBy;
static double Divide(double number, int divideBy)
    => number / divideBy;
