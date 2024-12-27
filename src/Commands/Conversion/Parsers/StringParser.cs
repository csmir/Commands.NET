﻿namespace Commands.Conversion
{
    internal sealed class StringParser : TypeParser<string>
    {
        public static StringParser Instance { get; } = new();

        public override ValueTask<ConvertResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (value is string str)
                return Success(str);

            return Success(value?.ToString());
        }
    }
}
