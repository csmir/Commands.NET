
using Commands;
using Commands.Samples;

var exit = Command.Define("exit")
    .Handler(() => Environment.Exit(0));

var mathCommands = CommandGroup.Define("math")
    .Components(
        Command.Define(Sum, "sum", "add"),
        Command.Define(Subtract, "subtract", "sub"),
        Command.Define(Multiply, "multiply", "mul"),
        Command.Define(Divide, "divide", "div")
    );

var manager = ComponentManager.Define()
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
