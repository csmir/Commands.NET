using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Core
{
    /// <summary>
    ///     
    /// </summary>
    public enum ScopeApproach
    {
        /// <summary>
        ///     
        /// </summary>
        Default = ByAsyncApproach,

        /// <summary>
        ///     
        /// </summary>
#pragma warning disable CA1069 // Enums values should not be duplicated
        ByAsyncApproach = 0,
#pragma warning restore CA1069 // Enums values should not be duplicated

        /// <summary>
        ///     
        /// </summary>
        OnlySync = 1,
        
        /// <summary>
        ///     
        /// </summary>
        OnlyAsync = 2,
    }
}
