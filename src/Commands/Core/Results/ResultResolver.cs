using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace Commands.Core
{
    /// <summary>
    ///     A container that implements an asynchronous functor to handle post-execution operations.
    /// </summary>
    public class ResultResolver()
    {
        private static readonly Lazy<ResultResolver> _i = new(() => new ResultResolver());

        /// <summary>
        ///     Gets or sets the handler responsible for post-execution failure handling.
        /// </summary>
        public Func<ICommandContext, ICommandResult, IServiceProvider, Task> FailHandle { get; set; }

        /// <summary>
        ///     Gets or sets the handler responsible for post-execution success handling.
        /// </summary>
        public Func<ICommandContext, ICommandResult, IServiceProvider, Task> SuccessHandle { get; set; }

        /// <summary>
        ///     Validates the state of the <see cref="FailHandle"/> and attempts to execute the delegate.
        /// </summary>
        /// <param name="context">Context of the current execution.</param>
        /// <param name="result">The result of the command, being successful or containing failure information.</param>
        /// <param name="scope">The provider used to register modules and inject services.</param>
        /// <returns>An awaitable <see cref="Task"/> that waits for the delegate to finish.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task TryHandleAsync(ICommandContext context, ICommandResult result, AsyncServiceScope scope)
        {
            if (result.Success())
            {
                await SuccessHandle?.Invoke(context, result, scope.ServiceProvider);
            }    
            else
            {
                await FailHandle?.Invoke(context, result, scope.ServiceProvider);
            }

            await scope.DisposeAsync();
        }

        internal static ResultResolver Default
        {
            get
            {
                return _i.Value;
            }
        }
    }
}
