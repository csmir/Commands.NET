using Commands.Conditions;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a conditional component, needing validation in order to become part of execution.
    /// </summary>
    public interface IConditional : INameable
    {
        /// <summary>
        ///     Gets an array of aliases for this component.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Gets if the component name is queryable.
        /// </summary>
        public bool IsQueryable { get; }

        /// <summary>
        ///     Gets the priority of the component.
        /// </summary>
        public byte Priority { get; }

        /// <summary>
        ///     Gets an array of <see cref="PreconditionAttribute"/>'s defined atop this component.
        /// </summary>
        public PreconditionAttribute[] Preconditions { get; }

        /// <summary>
        ///     Gets an array of <see cref="PostconditionAttribute"/>'s defined atop this component.
        /// </summary>
        public PostconditionAttribute[] PostConditions { get; }
    }
}
