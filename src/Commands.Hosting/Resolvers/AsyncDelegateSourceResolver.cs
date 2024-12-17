using System.ComponentModel;

namespace Commands.Resolvers
{
    /// <summary>
    ///     Represents a source resolver that invokes a delegate when the source is requested. This class cannot be inherited.
    /// </summary>
    /// <param name="func">The asynchronous delegate representing this operation.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class AsyncDelegateSourceResolver(
        Func<IServiceProvider, ValueTask<SourceResult>> func) : SourceResolver
    {
        private readonly Func<IServiceProvider, ValueTask<SourceResult>> _func = func;

        /// <inheritdoc/>
        public override ValueTask<SourceResult> Evaluate(IServiceProvider services, CancellationToken cancellationToken)
            => _func(services);
    }
}
