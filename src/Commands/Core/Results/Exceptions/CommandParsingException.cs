﻿using Commands.Parsing;

namespace Commands;

/// <summary>
///     Represents an exception that is thrown when a parser fails to parse input.
/// </summary>
public sealed class ParserException(ITypeParser parser, string reason)
    : Exception(reason)
{
    /// <summary>
    ///     Gets the parser that caused the exception.
    /// </summary>
    public ITypeParser Parser { get; } = parser;
}

/// <summary>
///     Represents an exception that is thrown when one or more parsers fail to parse input.
/// </summary>
public sealed class CommandParsingException(Command command, Exception? innerException = null)
    : Exception(null, innerException)
{
    /// <summary>
    ///     Gets the command that caused the exception.
    /// </summary>
    public Command Command { get; } = command;
}
