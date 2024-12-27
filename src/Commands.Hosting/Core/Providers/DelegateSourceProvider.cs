namespace Commands
{
    /// <summary>
    ///     Represents a source resolver that invokes a delegate when the source is requested. This class cannot be inherited.
    /// </summary>
    /// <param name="func">The asynchronous delegate representing this source operation.</param>
    public sealed class DelegateSourceProvider(
        Func<IServiceProvider, ValueTask<SourceResult>> func) : SourceProvider
    {
        /// <inheritdoc/>
        public override ValueTask<SourceResult> Receive(IServiceProvider services, CancellationToken cancellationToken)
        {
            if (Ready())
                return func(services);

            return Error("The source is not ready to be resolved.");
        }
    }
}
