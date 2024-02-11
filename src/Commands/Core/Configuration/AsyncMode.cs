using Commands.Conditions;
using Commands.TypeConverters;

namespace Commands
{
    /// <summary>
    ///     Defines a setting that tells the command execution pipeline to wait on command execution to finish or to slip thread.
    /// </summary>
    /// <remarks>
    ///     The asynchronous execution approach drastically changes the expected behavior of executing a command:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="Await"/> is the default setting and tells the pipeline to finish executing before returning control to the caller. 
    ///             This ensures that the execution will fully finish executing, whether it failed or not, before allowing another to be executed.
    ///         </item>
    ///         <item>
    ///             <see cref="Discard"/> is a setting to be treated with care. 
    ///             Instead of waiting for the full execution before returning control, the execution will return immediately after the entrypoint is called, slipping thread for the rest of execution. 
    ///             When more than one input source is expected to be handled, this is generally the advised method of execution. 
    ///         </item>
    ///     </list>
    /// </remarks>
    public enum AsyncMode
    {
        /// <summary>
        ///     The default option.
        /// </summary>
        Default = Await,

        /// <summary>
        ///     Tells the command execution pipeline to finish execution before returning control to the caller.
        /// </summary>
#pragma warning disable CA1069 // Enums values should not be duplicated
        Await = 0,
#pragma warning restore CA1069 // Enums values should not be duplicated

        /// <summary>
        ///     Tells the command execution pipeline to immediately slip thread and return to the caller without context.
        /// </summary>
        /// <remarks>
        ///     Changing to this setting, the following should be checked for thread-safety:
        ///     <list type="number">
        ///         <item>
        ///             Services, specifically those created as singleton or scoped to anything but a single command.
        ///         </item>
        ///         <item>
        ///             Implementations of <see cref="TypeConverterBase"/>, <see cref="TypeConverterBase{T}"/> and <see cref="PreconditionAttribute"/>.
        ///         </item>
        ///         <item>
        ///             Generic collections and objects with shared access.
        ///         </item>
        ///     </list>
        ///     For ensuring thread safety in any of the above situations, it is important to know what this actually means. 
        ///     <br/>
        ///     For more information, consider reading this article: <see href="https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices"/>
        /// </remarks>
        Discard = 1,
    }
}
