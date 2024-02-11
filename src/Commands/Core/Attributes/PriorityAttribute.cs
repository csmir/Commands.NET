using System.Diagnostics.CodeAnalysis;

namespace Commands
{
    /// <summary>
    ///     An attribute that can prioritize one result over another when multiple matches were found.
    /// </summary>
    /// <remarks>
    ///     By default, a command has a priority of 0. Higher values take priority, meaning a command with a priority of 1 will execute first if other commands have a priority of 0.
    /// </remarks>
    /// <remarks>
    ///     Creates a new <see cref="PriorityAttribute"/> with provided priority.
    /// </remarks>
    /// <param name="priority">The priority of this command, which can be between 0 and 255.</param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PriorityAttribute([DisallowNull] byte priority) : Attribute
    {
        /// <summary>
        ///     Gets the priority of a command, where higher values take priority over lower ones.
        /// </summary>
        public byte Priority { get; } = priority;
    }
}
