namespace Commands.Console.Core.Execution
{
    /// <summary>
    ///     Represents a module that can contain commands to execute, implementing <see cref="ModuleBase"/> with expanded functionality for console applications.
    /// </summary>
    public class ConsoleModuleBase : ModuleBase
    {

    }

    /// <summary>
    ///     Represents a module that can contain commands to execute, implementing <see cref="ModuleBase{TConsumer}"/> with expanded functionality for console applications.
    /// </summary>
    /// <typeparam name="TConsumer">The consumer of the command being executed.</typeparam>
    public class ConsoleModuleBase<TConsumer> : ModuleBase<TConsumer> 
        where TConsumer : ConsoleConsumerBase
    {

    }
}
