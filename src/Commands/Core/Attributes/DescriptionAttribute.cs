using Commands.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace Commands.Core
{
    /// <summary>
    ///     An attribute to give a description to a command, argument or module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        /// <summary>
        ///     The description of this command, argument or module.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Sets up a new <see cref="DescriptionAttribute"/> with provided value.
        /// </summary>
        /// <param name="description">The description for a command, argument or module.</param>
        public DescriptionAttribute([DisallowNull] string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                ThrowHelpers.ThrowInvalidArgument(description);

            Description = description;
        }
    }
}
