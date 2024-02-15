using Commands.Conditions;

namespace Commands.Reflection
{
    /// <summary>
    ///     Reveals information about a conditional component, needing validation in order to become part of execution.
    /// </summary>
    public interface ISearchable : IScoreable
    {
        /// <summary>
        ///     Gets an array of aliases for this component.
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        ///     Gets if the component name is queryable.
        /// </summary>
        public bool IsSearchable { get; }

        /// <summary>
        ///     Gets if the component is the default of a module-layer.
        /// </summary>
        public bool IsDefault { get; }

        /// <summary>
        ///     Gets the priority of the component.
        /// </summary>
        public float Priority { get; }

        /// <summary>
        ///     Gets the score of the component.
        /// </summary>
        /// <remarks>
        ///     Score defines the match priority of a component over another. This score is computed based on complexity, argument length and conversion.
        /// </remarks>
        public float Score { get; }

        /// <summary>
        ///     Gets the root module.
        /// </summary>
        /// <remarks>
        ///     Will be <see langword="null"/> if the component is not nested in a module.
        /// </remarks>
        public ModuleInfo? Module { get; }

        /// <summary>
        ///     Gets an array of <see cref="PreconditionAttribute"/>'s defined atop this component.
        /// </summary>
        public PreconditionAttribute[] Preconditions { get; }

        /// <summary>
        ///     Gets an array of <see cref="PostconditionAttribute"/>'s defined atop this component.
        /// </summary>
        public PostconditionAttribute[] PostConditions { get; }

        /// <summary>
        ///     Gets the invocation target of this component.
        /// </summary>
        public IInvoker Invoker { get; }
    }
}
