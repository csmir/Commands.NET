﻿using System.ComponentModel;

namespace Commands.Conversion
{
    /// <summary>
    ///     Represents a converter that invokes a delegate when parameter conversion of its type <typeparamref name="T"/> occurs. This class cannot be inherited.
    /// </summary>
    /// <typeparam name="T">The convertible type that this converter should convert to.</typeparam>
    /// <param name="func">The delegate that is invoked when the conversion is requested.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DelegateConverter<T>(
        Func<ICallerContext, IArgument, object?, IServiceProvider, ConvertResult> func)
        : TypeParser<T>
    {
        private readonly Func<ICallerContext, IArgument, object?, IServiceProvider, ConvertResult> _func = func;

        /// <inheritdoc />
        public override async ValueTask<ConvertResult> Parse(ICallerContext caller, IArgument argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            return _func(caller, argument, value, services);
        }
    }
}
