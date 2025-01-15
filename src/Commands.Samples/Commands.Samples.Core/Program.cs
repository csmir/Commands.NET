
using Commands;
using Commands.Samples;

var exit = Command.From("exit")
    .Handler(() => Environment.Exit(0));

var mathCommands = CommandGroup.From("math")
    .Components(
        Command.From(Sum, "sum", "add"),
        Command.From(Subtract, "subtract", "sub"),
        Command.From(Multiply, "multiply", "mul"),
        Command.From(Divide, "divide", "div")
    );

var manager = ComponentManager.From()
    .Components(exit, mathCommands)
    .Type<HelpModule>()
    .ToManager();

manager.TryExecute(new ConsoleContext(), args);

static double Sum(double number, int sumBy)
    => number + sumBy;
static double Subtract(double number, int subtractBy)
    => number - subtractBy;
static double Multiply(double number, int multiplyBy)
    => number * multiplyBy;
static double Divide(double number, int divideBy)
    => number / divideBy;
