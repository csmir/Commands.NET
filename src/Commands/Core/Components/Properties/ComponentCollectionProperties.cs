namespace Commands;

public sealed class ComponentCollectionProperties
{
    public ComponentCollectionProperties()
    {

    }

    public ComponentCollectionProperties Component(IComponent component)
    {

    }

    public ComponentCollectionProperties Component(DynamicType componentType)
    {

    }

    public ComponentCollectionProperties Component<TComponent>()
        where TComponent : CommandModule
    {

    }

    public ComponentCollectionProperties Components(params IComponent[] componentDefinitions)
    {

    }

    public ComponentCollectionProperties Components(params DynamicType[] componentTypes)
    {

    }

    public ComponentCollectionProperties Handler(ResultHandler handler)
    {

    }

    public ComponentCollectionProperties Handlers(params ResultHandler[] handlers)
    {

    }

    public ComponentCollectionProperties Configuration(ComponentConfiguration configuration)
    {

    }

    public ComponentCollection ToCollection()
    {

    }

    public static implicit operator ComponentCollection(ComponentCollectionProperties properties)
        => properties.ToCollection();
}
