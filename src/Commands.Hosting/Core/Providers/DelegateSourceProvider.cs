﻿namespace Commands
{
    /// <summary>
    ///     Represents a source resolver that invokes a delegate when the source is requested. This class cannot be inherited.
    /// </summary>
    /// <param name="func">The delegate representing this source operation.</param>
    public sealed class DelegateSourceProvider(
        Func<IServiceProvider, SourceResult> func) : SourceProvider
    {
        private readonly Func<IServiceProvider, SourceResult> _func = func;

        /// <inheritdoc />
        public override ValueTask<SourceResult> Wait(IServiceProvider services, CancellationToken cancellationToken)
        {
            if (Ready())
                return _func(services);
            return Error("The source is not ready to be resolved.");
        }
    }
}
