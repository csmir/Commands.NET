namespace Commands;

public interface IComponentProvider
{
    public ComponentTree Components { get; }

    public IReadOnlyCollection<ResultHandler> Handlers { get; }
    
    public Task Execute<T>(T caller, CommandOptions? options = null)
        where T : class, ICallerContext;
}
