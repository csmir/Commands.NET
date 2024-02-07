using System.Reflection;

namespace Commands.Core
{
    /// <summary>
    ///     A context for build modules and commands for the <see cref="CommandManager"/> to use.
    /// </summary>
    public struct BuildingContext()
    {
        /// <summary>
        ///     Gets or sets a collection of assemblies that are to be used to discover created modules.
        /// </summary>
        /// <remarks>
        ///     Default: [ <see cref="Assembly.GetEntryAssembly"/> ]
        /// </remarks>
        public Assembly[] Assemblies { get; set; } = [Assembly.GetEntryAssembly()];
    }
}
