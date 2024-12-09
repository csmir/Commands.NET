﻿namespace Commands
{
    /// <summary>
    ///     An attribute that can prioritize one result over another when multiple matches were found.
    /// </summary>
    /// <remarks>
    ///     By default, a command has a priority calculated by the following logic:
    ///     <list type="number">
    ///         <item>For each parameter in the signature, the priority is incremented by <c>1</c>.</item>
    ///         <item>If a parameter is optional, the value is decremented by <c>0.5</c>.</item>
    ///         <item>If a parameter is remainder, the value is decremented by <c>0.25</c>.</item>
    ///         <item>If a parameter is nullable, the value is decremented by <c>0.25</c>.</item>
    ///         <item>If a parameter has a known type converter, the value is incremenented by <c>1</c>.</item>
    ///     </list>
    ///     Higher values take priority, meaning a command with a priority of <c>1</c> will execute first if other commands have a priority of <c>0</c>.
    /// </remarks>
    /// <param name="priority">The priority of this command, which adds to the score calculation of the command.</param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PriorityAttribute(float priority) : Attribute
    {
        /// <summary>
        ///     Gets the priority of a command, where higher values take priority over lower ones.
        /// </summary>
        public float Priority { get; } = priority;
    }
}
