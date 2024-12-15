﻿using Commands.Reflection;

namespace Commands.Converters
{
    internal sealed class StringTypeConverter : TypeConverterBase<string>
    {
        public static StringTypeConverter Instance { get; } = new();

        public override async ValueTask<ConvertResult> Evaluate(CallerContext consumer, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (value is string str)
            {
                return Success(str);
            }

            return Success(value?.ToString()!);
        }
    }
}
