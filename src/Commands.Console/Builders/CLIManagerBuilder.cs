namespace Commands.Builders;

/// <summary>
///     Represents a set of extensions for the <see cref="ComponentManagerBuilder"/> class.
/// </summary>
public static class CLIManagerBuilder
{
    /// <summary>
    ///     Adds a new <see cref="Delegate"/> based command to the list of <see cref="IManagerBuilder.Components"/>. 
    /// </summary>
    /// <remarks>
    ///     This overload sets a default command name. This command is meant to be used as a default command for the environment, and will be triggered if no CLI arguments are provided.
    /// </remarks>
    /// <param name="builder">The command builder to add the command to.</param>
    /// <param name="commandAction">The action of the command. Delegate commands are adviced to set the first parameter to be <see cref="CommandContext{T}"/>, which holds scope and execution information of the created command during its execution. </param>
    /// <returns>The same <see cref="ComponentManagerBuilder"/> for call-chaining.</returns>
    public static IManagerBuilder AddCommand(this IManagerBuilder builder, Delegate commandAction)
    {
        var coreCommandName = builder.Configuration.Properties["CLIDefaultOverloadName"] as string
            ?? throw new NotSupportedException($"The default CLI overload name is unavailable in the current context.");

        builder.AddCommand(coreCommandName, commandAction, []);

        return builder;
    }

    /// <summary>
    ///     Builds the underlying <see cref="IExecutionProvider"/> and runs it with the provided <see cref="CLIOptions{T}"/>.
    /// </summary>
    /// <param name="builder">The command builder to build into a manager.</param>
    /// <param name="options">The options that set up a single command execution.</param>
    /// <returns>An asynchronous <see cref="ValueTask"/> containing the state of the command execution.</returns>
    public static void Run<T>(this IManagerBuilder builder, CLIOptions<T> options)
        where T : ConsoleCallerContext
    {
        var coreCommandName = builder.Configuration.Properties["CLIDefaultOverloadName"] as string
            ?? throw new NotSupportedException($"The default CLI overload name is unavailable in the current context.");

        var manager = builder.Build();

        if (options.Arguments == null || options.Arguments.Length == 0)
            options.Arguments = [coreCommandName];

        manager.TryExecute(options.Caller, ArgumentArray.Read(options.Arguments), options.Options);
    }

    /// <summary>
    ///     Builds the underlying <see cref="IExecutionProvider"/> and runs it with the provided <see cref="CLIOptions{T}"/>.
    /// </summary>
    /// <param name="builder">The command builder to build into a manager.</param>
    /// <param name="caller">The caller that represents the source of this execution.</param>
    /// <param name="args">The CLI arguments that should be used to execute a command.</param>
    /// <returns>An asynchronous <see cref="ValueTask"/> containing the state of the command execution. This task should be awaited </returns>
    public static void Run<T>(this IManagerBuilder builder, T caller, string[] args)
        where T : ConsoleCallerContext
    {
        var options = new CLIOptions<T>(caller)
        {
            Arguments = args
        };

        builder.Run(options);
    }
}
