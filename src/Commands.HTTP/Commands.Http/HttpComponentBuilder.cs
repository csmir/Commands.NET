using Commands.Hosting;
using System.ComponentModel;

namespace Commands.Http;

/// <summary>
///     Represents a <see cref="ComponentBuilder"/> that extends configuration for HTTP based components.
/// </summary>
public sealed class HttpComponentBuilder : ComponentBuilder
{
    /// <inheritdoc />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override ComponentBuilder SetDefaults()
    {
        base.SetDefaults();

        ServiceDictionary["CommandExecutionFactory"] = new TypeWrapper(typeof(HttpCommandExecutionFactory));

        return this;
    }
}
