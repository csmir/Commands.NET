using Commands.Builders;
using Commands.Conversion;

namespace Commands;

/// <summary>
///     Represents the manager for executing CLI commands.
/// </summary>
public static class CLIManager
{
    /// <summary>
    ///     Runs the provided <see cref="CLIOptions{T}"/> as a command.
    /// </summary>
    /// <param name="provider">The <see cref="IExecutionProvider"/> instance that should be used to run the CLI command.</param>
    /// <param name="options">The options that set up a single command execution.</param>
    /// <returns>An asynchronous <see cref="ValueTask"/> containing the state of the command execution.</returns>
    public static void Run<T>(this IExecutionProvider provider, CLIOptions<T> options)
        where T : ConsoleCallerContext, new()
    {
        options.Caller ??= new T();

        provider.TryExecute(options.Caller, ArgumentArray.Read(options.Arguments), options.Options);
    }

    /// <summary>
    ///     Runs the current builder with the provided caller.
    /// </summary>
    /// <param name="provider">The <see cref="IExecutionProvider"/> instance that should be used to run the CLI command.</param>
    /// <param name="caller">The caller that represents the source of this execution.</param>
    /// <param name="args">The CLI arguments that should be used to execute a command.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> containing the state of the command execution.</returns>
    public static void Run<T>(this IExecutionProvider provider, T caller, string[] args)
        where T : ConsoleCallerContext, new()
    {
        var options = new CLIOptions<T>(caller)
        {
            Arguments = args
        };

        provider.Run(options);
    }

    /// <summary>
    ///     Creates a builder that is responsible for setting up all required arguments to discover and populate the <see cref="IExecutionProvider"/>.
    /// </summary>
    /// <returns>A new <see cref="ComponentManagerBuilder"/> that builds into a new instance of <see cref="IExecutionProvider"/> based on the provided arguments.</returns>
    public static IManagerBuilder CreateBuilder()
    {
        var configuration = new ComponentConfigurationBuilder();

        configuration.Properties["CLIDefaultOverloadName"] = "env-core";

        configuration.AddParser(new ColorTypeParser());

        return new ComponentManagerBuilder()
        {
            Configuration = configuration
        };
    }
}
