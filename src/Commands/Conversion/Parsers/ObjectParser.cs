namespace Commands.Conversion
{
    // This converter is used exclusively for enumerable conversion.
    internal sealed class ObjectParser : TypeParser<object>
    {
        public override ValueTask<ConvertResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
            => Success(value);
    }
}
