using Commands.Builders;

namespace Commands
{
    /// <summary>
    ///     The root type serving as a basis for all operations and functionality as provided by Commands.NET.
    ///     To learn more about use of this type and other features of Commands.NET, check out the README on GitHub: <see href="https://github.com/csmir/Commands.NET"/>
    /// </summary>
    /// <remarks>
    ///     To start using this tree, call <see cref="ComponentTree.CreateBuilder"/> and configure it using the fluent API's implemented by <see cref="ITreeBuilder"/>.
    /// </remarks>
    public interface IComponentTree : IComponentCollection
    {
        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">An unparsed input that is expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public ValueTask Execute<T>(
            T caller, string args, CommandOptions? options = null)
            where T : ICallerContext;

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        ///     The arguments intended for searching for a target need to be <see cref="string"/>, as <see cref="ModuleInfo"/> and <see cref="CommandInfo"/> store their aliases as <see cref="string"/> values.
        /// </remarks>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public ValueTask Execute<T>(
            T caller, IEnumerable<object> args, CommandOptions? options = null)
            where T : ICallerContext;

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public ValueTask Execute<T>(
            T caller, IEnumerable<KeyValuePair<string, object?>> args, CommandOptions? options = null)
            where T : ICallerContext;

        /// <summary>
        ///     Attempts to execute a command based on the provided <paramref name="args"/>.
        /// </summary>
        /// <param name="caller">A command caller that persist for the duration of the execution pipeline, serving as a metadata container.</param>
        /// <param name="args">A parsed set of arguments that are expected to discover, populate and invoke a target command.</param>
        /// <param name="options">A collection of options that determines pipeline logic.</param>
        /// <returns>An awaitable <see cref="ValueTask"/> hosting the state of execution. This task should be awaited, even if <see cref="CommandOptions.DoAsynchronousExecution"/> is set to <see langword="true"/>.</returns>
        public ValueTask Execute<T>(
            T caller, ArgumentEnumerator args, CommandOptions options)
            where T : ICallerContext;
    }
}
