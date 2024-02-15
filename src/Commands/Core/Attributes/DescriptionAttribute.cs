using Commands.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace Commands
{
    /// <summary>
    ///     An attribute to give a description to a target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DescriptionAttribute : Attribute
    {
        /// <summary>
        ///     The description of the target.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Creates a new <see cref="DescriptionAttribute"/> with provided description.
        /// </summary>
        /// <param name="description">The description for the target.</param>
        public DescriptionAttribute([DisallowNull] string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                ThrowHelpers.ThrowInvalidArgument(description);
            }

            Description = description;
        }
    }
}
