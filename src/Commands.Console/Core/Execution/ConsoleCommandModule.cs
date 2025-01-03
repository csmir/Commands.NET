using Spectre.Console;

namespace Commands;

/// <summary>
///     Represents a module that can contain commands to execute, implementing <see cref="CommandModule{TConsumer}"/> with expanded functionality for console applications.
/// </summary>
/// <typeparam name="TCaller">The consumer of the command being executed.</typeparam>
public class ConsoleCommandModule<TCaller> : CommandModule<TCaller>
    where TCaller : ConsoleCallerContext
{
    /// <summary>
    ///     Sends a question to the console and returns the response.
    /// </summary>
    /// <remarks>
    ///     This method waits for the response of a console read. This should not be used when <see cref="CommandOptions.AsynchronousExecution"/> is used, as it will take over the console input until a response is provided.
    /// </remarks>
    /// <param name="question">The question that should be asked to the console.</param>
    /// <returns>The response to the question.</returns>
    public virtual string Ask(string question)
        => Caller.Ask(question);

    /// <summary>
    ///     Asks the console to confirm a question with yes or no.
    /// </summary>
    /// <remarks>
    ///     This method waits for the response of a console read. This should not be used when <see cref="CommandOptions.AsynchronousExecution"/> is used, as it will take over the console input until a response is provided.
    /// </remarks>
    /// <param name="question">The question that should be asked to the console.</param>
    /// <returns><see langword="true"/> if the question was responded with with 'Y' or 'Yes'. <see langword="false"/> if the response is 'N', 'No' or if the sequence was escaped otherwise.</returns>
    public virtual bool Confirm(string question)
        => Caller.Confirm(question);

    /// <summary>
    ///     Asks the console to respond to a prompt.
    /// </summary>
    /// <remarks>
    ///     This method waits for the response of a console read. This should not be used when <see cref="CommandOptions.AsynchronousExecution"/> is used, as it will take over the console input until a response is provided.
    /// </remarks>
    /// <param name="prompt">The prompt that should be responded to by the console.</param>
    /// <returns>The result of the text prompt.</returns>
    public virtual T Prompt<T>(TextPrompt<T> prompt)
        => Caller.Prompt(prompt);

    /// <summary>
    ///     Asks the console to choose an item in a selection.
    /// </summary>
    /// <remarks>
    ///     This method waits for the response of a console read. This should not be used when <see cref="CommandOptions.AsynchronousExecution"/> is used, as it will take over the console input until a response is provided.
    /// </remarks>
    /// <param name="prompt"></param>
    /// <returns>The result of the selection.</returns>
    public virtual T Select<T>(SelectionPrompt<T> prompt)
        where T : notnull
        => Caller.Select(prompt);

    /// <summary>
    ///     Creates a new line in the console.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response. This call does not need to be awaited, running async if not.</returns>
    public virtual Task Send()
        => Caller.NewLine();
}
