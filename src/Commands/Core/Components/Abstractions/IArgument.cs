﻿using Commands.Conversion;

namespace Commands
{
    /// <summary>
    ///     Reveals information about an invocation argument of a command or any complex member.
    /// </summary>
    public interface IArgument : IScoreable, IParameter
    {
        /// <summary>
        ///     Gets if this argument is the query remainder or not.
        /// </summary>
        public bool IsRemainder { get; }

        /// <summary>
        ///     Gets if this argument is a collection type or not.
        /// </summary>
        public bool IsCollection { get; }

        /// <summary>
        ///     Gets the converter for this argument.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if <see cref="Type"/> is <see cref="string"/>, <see cref="object"/>, or if this argument is <see cref="ComplexArgumentInfo"/>.
        /// </remarks>
        public TypeParser? Converter { get; }
    }
}
