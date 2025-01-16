using System.ComponentModel;

namespace Commands;

/// <summary>
///     A utility class for working with <see cref="IExecuteResult"/> instances.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ResultUtilities
{
    /// <summary>
    ///     Gets the message of the exception of the result.
    /// </summary>
    /// <param name="result">The result for which an exception occurred.</param>
    /// <returns>The message of the innermost exception this result contains. <see langword="null"/> if no exception is present on the result.</returns>
    public static string? GetMessage(this IExecuteResult result)
    {
        static Exception Unfold(Exception exception)
        {
            if (exception.InnerException != null)
                return Unfold(exception.InnerException);

            return exception;
        }

        if (result.Exception != null)
            return Unfold(result.Exception).Message;

        return null;
    }
}
