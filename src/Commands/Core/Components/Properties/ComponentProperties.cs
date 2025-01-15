using System.ComponentModel;

namespace Commands;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ComponentProperties
{
    public abstract IComponent ToComponent(CommandGroup? parent = null, ComponentConfiguration? configuration = null);
}
