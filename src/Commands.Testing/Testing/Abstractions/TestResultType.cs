namespace Commands.Testing;

/// <summary>
///     The type of result returned from a test. This is represented as the different types of <see cref="IResult"/> that can be yielded by the command execution pipeline.
/// </summary>
public enum TestResultType : int
{
    /// <summary>
    ///     The test failed because the provided input arguments do not match the expected arguments.
    /// </summary>
    /// <remarks>
    ///     This value represents a failed <see cref="MatchResult"/>
    /// </remarks>
    MatchFailure,

    /// <summary>
    ///     The test failed because the input cannot be converted to the expected type of one or more parameters of the command.
    /// </summary>
    ParseFailure,

    /// <summary>
    ///     The test failed because one or more execution conditions failed to succeed.
    /// </summary>
    ConditionFailure,

    /// <summary>
    ///     The test failed because an exception occurred during invocation.
    /// </summary>
    InvocationFailure,

    /// <summary>
    ///     The test succeeded.
    /// </summary>
    Success
}
