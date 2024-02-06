using System.Diagnostics.CodeAnalysis;

namespace Commands.Core
{
    /// <summary>
    ///     A container that contains metadata and logging access for a command attempted to be executed.
    /// </summary>
    /// <remarks>
    ///     It is generally not adviced to implement this interface directly. Instead, consider implementing <see cref="CommandContext"/>.
    /// </remarks>
    public interface ICommandContext
    {
        internal bool TryGetFallback([NotNullWhen(true)] out ICommandResult result);

        internal void TrySetFallback(ICommandResult result);
    }
}
