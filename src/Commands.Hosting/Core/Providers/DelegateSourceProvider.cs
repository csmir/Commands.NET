using System.ComponentModel;

namespace Commands
{
    /// <summary>
    ///     Represents a source resolver that invokes a delegate when the source is requested. This class cannot be inherited.
    /// </summary>
    /// <param name="func">The delegate representing this operation.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DelegateSourceProvider(
        Func<IServiceProvider, SourceResult> func) : SourceProvider
    {
        private readonly Func<IServiceProvider, SourceResult> _func = func;

        /// <inheritdoc />
        public override ValueTask<SourceResult> Evaluate(IServiceProvider services, CancellationToken cancellationToken)
            => new(_func(services));
    }
}
