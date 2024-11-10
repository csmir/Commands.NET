using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Commands.Helpers
{
    /// <summary>
    ///     A set of helper methods to throw exceptions for the invalid state of objects or operations.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ThrowHelpers
    {
        /// <summary>
        ///     Throws <see cref="InvalidOperationException"/> for any use.
        /// </summary>
        /// <param name="failureMessage">The message to throw.</param>
        /// <exception cref="InvalidOperationException"></exception>
        [DoesNotReturn]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ThrowInvalidOperation(string failureMessage)
        {
            throw new InvalidOperationException(failureMessage);
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException"/> for value population.
        /// </summary>
        /// <param name="value">The object for which to throw</param>
        /// <param name="arg">Do not set.</param>
        /// <exception cref="ArgumentException"></exception>
        [DoesNotReturn]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ThrowInvalidArgument(
            object? value, [CallerArgumentExpression(nameof(value))] string? arg = null)
        {
            // should resolve overload when a collection is null or empty.
            if (value is ICollection or IEnumerable)
            {
                // if invalidarg is called on not null collection, it must mean it needs a value.
                if (value != null)
                {
                    throw new ArgumentException(
                        message: "Provided collection must be initialized with at least one argument. It is not allowed to be null or empty.",
                        paramName: arg);
                }

                // thrown when collection is null.
                throw new ArgumentException(
                    message: "Provided collection must be initialized. It is not allowed to be null.",
                    paramName: arg);
            }

            // if value is a string, this check ensures that more data is provided at throw, regarding the state of the string.
            if (value is string)
            {
                throw new ArgumentException(
                    message: "Provided string must carry at least one character that is not whitespace. It is not allowed to be null or empty.",
                    paramName: arg);
            }

            // throw argument null with custom message in the situation where none of the above overloads are relevant.
            throw new ArgumentException(
                message: "Provided argument must carry a value. It is not allowed to be null.",
                paramName: arg);
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException"/> for regex match failure.
        /// </summary>
        /// <param name="value">The value for which to throw.</param>
        /// <param name="arg"></param>
        /// <exception cref="ArgumentException"></exception>
        [DoesNotReturn]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ThrowNotMatched(
            string? value, [CallerArgumentExpression(nameof(value))] string? arg = null)
        {
            throw new ArgumentException(
                message: "Value does not match expression.",
                paramName: arg);
        }

        /// <summary>
        ///     Throws <see cref="ArgumentException"/> for duplicate values in a collection.
        /// </summary>
        /// <param name="value">The object for which to throw.</param>
        /// <param name="arg">Do not set.</param>
        /// <exception cref="ArgumentException"></exception>
        [DoesNotReturn]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ThrowNotDistinct(
            object? value, [CallerArgumentExpression(nameof(value))] string? arg = null)
        {
            throw new ArgumentException(
                message: "Provided collection cannot contain duplicate values.",
                paramName: arg);
        }
    }
}
