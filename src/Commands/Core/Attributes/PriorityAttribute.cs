using System.Diagnostics.CodeAnalysis;

namespace Commands
{
    /// <summary>
    ///     An attribute that can prioritize one result over another when multiple matches were found.
    /// </summary>
    /// <remarks>
    ///     By default, a command has a priority calculated by the following logic:
    ///     <list type="number">
    ///         <item>For each parameter in the signature, the priority is incremented by 1.</item>
    ///         <item>If a parameter is optional, the value is decremented by 0.5.</item>
    ///         <item>If a parameter is remainder, the value is decremented by 0.25.</item>
    ///         <item>If a parameter is nullable, the value is decremented by 0.25.</item>
    ///         <item>If a parameter has a known type converter, the value is incremenented by 1.</item>
    ///     </list>
    ///     Higher values take priority, meaning a command with a priority of 1 will execute first if other commands have a priority of 0.
    /// </remarks>
    /// <param name="priority">The priority of this command, which can be between 0 and 255.</param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PriorityAttribute([DisallowNull] float priority) : Attribute
    {
        /// <summary>
        ///     Gets the priority of a command, where higher values take priority over lower ones.
        /// </summary>
        public float Priority { get; } = priority;
    }
}
