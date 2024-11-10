using Spectre.Console;

namespace Commands
{
    /// <summary>
    ///     Represents a module that can contain commands to execute, implementing <see cref="ModuleBase{TConsumer}"/> with expanded functionality for console applications.
    /// </summary>
    /// <typeparam name="TConsumer">The consumer of the command being executed.</typeparam>
    public class ConsoleModuleBase<TConsumer> : ModuleBase<TConsumer>
        where TConsumer : ConsoleConsumerBase
    {
        /// <summary>
        ///     Sends a question to the console and returns the response.
        /// </summary>
        /// <remarks>
        ///     This method waits for the response of a console read. This should not be used when <see cref="AsyncMode.Async"/> is used, as it will take over the console input until a response is provided.
        /// </remarks>
        /// <param name="question">The question that should be asked to the console.</param>
        /// <returns>The response to the question.</returns>
        public virtual string Ask(string question)
        {
            return Consumer.Ask(question);
        }

        /// <summary>
        ///     Asks the console to confirm a question with yes or no.
        /// </summary>
        /// <remarks>
        ///     This method waits for the response of a console read. This should not be used when <see cref="AsyncMode.Async"/> is used, as it will take over the console input until a response is provided.
        /// </remarks>
        /// <param name="question">The question that should be asked to the console.</param>
        /// <returns><see langword="true"/> if the question was responded with with 'Y' or 'Yes'. <see langword="false"/> if the response is 'N', 'No' or if the sequence was escaped otherwise.</returns>
        public virtual bool Confirm(string question)
        {
            return Consumer.Confirm(question);
        }

        /// <summary>
        ///     Asks the console to respond to a prompt.
        /// </summary>
        /// <remarks>
        ///     This method waits for the response of a console read. This should not be used when <see cref="AsyncMode.Async"/> is used, as it will take over the console input until a response is provided.
        /// </remarks>
        /// <param name="prompt">The prompt that should be responded to by the console.</param>
        /// <returns>The result of the text prompt.</returns>
        public virtual string Prompt(TextPrompt<string> prompt)
        {
            return Consumer.Prompt(prompt);
        }

        /// <summary>
        ///     Asks the console to choose an item in a selection.
        /// </summary>
        /// <remarks>
        ///     This method waits for the response of a console read. This should not be used when <see cref="AsyncMode.Async"/> is used, as it will take over the console input until a response is provided.
        /// </remarks>
        /// <param name="prompt"></param>
        /// <returns>The result of the selection.</returns>
        public virtual string Select(SelectionPrompt<string> prompt)
        {
            return Consumer.Select(prompt);
        }
    }
}
