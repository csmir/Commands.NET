using Spectre.Console;
using Spectre.Console.Rendering;

namespace Commands;

/// <summary>
///     Represents a caller that represents the current console window that the application is running in.
/// </summary>
public class ConsoleCallerContext : ICallerContext
{
    /// <summary>
    ///     Gets the console that the caller should write to.
    /// </summary>
    public IAnsiConsole Console { get; }

    /// <summary>
    ///     Creates a new <see cref="ConsoleCallerContext"/> with the default <see cref="AnsiConsole.Console"/>.
    /// </summary>
    public ConsoleCallerContext()
        => Console = AnsiConsole.Console;

    /// <summary>
    ///     Creates a new <see cref="ConsoleCallerContext"/> with the specified <see cref="IAnsiConsole"/>.
    /// </summary>
    /// <param name="console">The console that should be used to send messages to the caller.</param>
    public ConsoleCallerContext(IAnsiConsole console)
        => Console = console;

    /// <summary>
    ///     Sends a message to the console.
    /// </summary>
    /// <param name="response">The message that should be sent in response to the console.</param>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response. This call does not need to be awaited, running async if not.</returns>
    public Task Respond(object? response)
    {
        switch (response)
        {
            case IRenderable renderable:
                Console.Write(renderable);
                break;
            case Exception ex:
                Console.WriteException(ex);
                break;
            case FormattableString formattedString:
                Console.MarkupLineInterpolated(formattedString);
                break;
            case string str:
                Console.WriteLine(str);
                break;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Creates a new line in the console.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/> containing the state of the response. This call does not need to be awaited, running async if not.</returns>
    public Task Send()
    {
        Console.WriteLine();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Sends an exception to the console.
    /// </summary>
    /// <param name="exception">The exception that should be beautified in the console.</param>
    public virtual void SendException(Exception exception)
        => Console.WriteException(exception);

    /// <summary>
    ///     Sends a question to the console and returns the response.
    /// </summary>
    /// <remarks>
    ///     This method waits for the response of a console read. This should not be used when <see cref="CommandOptions.AsynchronousExecution"/> is used, as it will take over the console input until a response is provided.
    /// </remarks>
    /// <param name="question">The question that should be asked to the console.</param>
    /// <returns>The response to the question.</returns>
    public virtual string Ask(string question)
        => Console.Ask<string>($"{question}");

    /// <summary>
    ///     Asks the console to confirm a question with yes or no.
    /// </summary>
    /// <remarks>
    ///     This method waits for the response of a console read. This should not be used when <see cref="CommandOptions.AsynchronousExecution"/> is used, as it will take over the console input until a response is provided.
    /// </remarks>
    /// <param name="question">The question that should be asked to the console.</param>
    /// <returns><see langword="true"/> if the question was responded with with 'Y' or 'Yes'. <see langword="false"/> if the response is 'N', 'No' or if the sequence was escaped otherwise.</returns>
    public virtual bool Confirm(string question)
        => Console.Confirm($"{question}");

    /// <summary>
    ///     Asks the console to respond to a prompt.
    /// </summary>
    /// <remarks>
    ///     This method waits for the response of a console read. This should not be used when <see cref="CommandOptions.AsynchronousExecution"/> is used, as it will take over the console input until a response is provided.
    /// </remarks>
    /// <param name="prompt">The prompt that should be responded to by the console.</param>
    /// <returns>The result of the text prompt.</returns>
    public virtual T Prompt<T>(TextPrompt<T> prompt)
        => Console.Prompt(prompt);

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
        => Console.Prompt(prompt);
}
