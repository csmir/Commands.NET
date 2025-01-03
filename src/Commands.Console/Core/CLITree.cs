﻿using Commands.Builders;
using Commands.Conversion;

namespace Commands;

/// <summary>
///     Represents the tree for executing CLI commands.
/// </summary>
public static class CLITree
{
    /// <summary>
    ///     Runs the provided <see cref="CLIOptions{T}"/> as a command.
    /// </summary>
    /// <param name="tree">The <see cref="IComponentTree"/> instance that should be used to run the CLI command.</param>
    /// <param name="options">The options that set up a single command execution.</param>
    /// <returns>An asynchronous <see cref="ValueTask"/> containing the state of the command execution.</returns>
    public static void Run<T>(this IComponentTree tree, CLIOptions<T> options)
        where T : ConsoleCallerContext, new()
    {
        options.Caller ??= new T();

        tree.Execute(options.Caller, ArgumentArray.Read(options.Arguments), options.Options);
    }

    /// <summary>
    ///     Runs the current builder with the provided caller.
    /// </summary>
    /// <param name="tree">The <see cref="IComponentTree"/> instance that should be used to run the CLI command.</param>
    /// <param name="caller">The caller that represents the source of this execution.</param>
    /// <param name="args">The CLI arguments that should be used to execute a command.</param>
    /// <returns>An awaitable <see cref="ValueTask"/> containing the state of the command execution.</returns>
    public static void Run<T>(this IComponentTree tree, T caller, string[] args)
        where T : ConsoleCallerContext, new()
    {
        var options = new CLIOptions<T>(caller)
        {
            Arguments = args
        };

        tree.Run(options);
    }

    /// <summary>
    ///     Creates a builder that is responsible for setting up all required arguments to discover and populate the <see cref="IComponentTree"/>.
    /// </summary>
    /// <returns>A new <see cref="ComponentTreeBuilder"/> that builds into a new instance of <see cref="IComponentTree"/> based on the provided arguments.</returns>
    public static ITreeBuilder CreateBuilder()
    {
        var configuration = new ComponentConfigurationBuilder();

        configuration.Properties["CLIDefaultOverloadName"] = "env-core";

        configuration.AddParser(new ColorTypeParser());

        return new ComponentTreeBuilder()
        {
            Configuration = configuration
        };
    }
}
