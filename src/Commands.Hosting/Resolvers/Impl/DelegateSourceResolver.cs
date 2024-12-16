namespace Commands.Resolvers
{
    internal sealed class DelegateSourceResolver(
        Func<SourceResult> func) : SourceResolver
    {
        private readonly Func<SourceResult> _func = func;

        public override ValueTask<SourceResult> Evaluate(CancellationToken cancellationToken)
        {
            return new ValueTask<SourceResult>(_func());
        }
    }
}
