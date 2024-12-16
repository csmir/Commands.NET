Command results are handled by implementations of the `ICommandResult` interface.
When handling results, the different types of results are handled by the `ResultResolverBase` implementations. This document elaborates on how this process works.

- [Result types](#result-types)
- [Handling results](#handling-results)
- [Complex results](#complex-results)
- [Lightweight implementation](#lightweight-implementation)

## Result types

Amongst the different types of results, the scenario is different for each variant:

> Each result implements a `Success` property, which is used to determine if the result was successful or not. 
If `false`, the `Exception` property will contain the exception that was thrown, never being `null` on failure.

- `SearchResult` is returned in the search operation, returning discovered commands if any. It will return a failed result if:
  - No commands or modules were found.
  - A module was found, but no methods are available to execute.

- `MatchResult` is returned in the match operation, returning the commands that are ready for execution if any. It will return a failed result if:
  - An argument mismatch occurred between the command and the input.
  - A type conversion between the input and the target type of an argument failed.

- `ConditionResult` is returned in the condition evaluation, returning the result of the condition check. It will return a failed result if one or multiple conditions were unmet.

- `InvokeResult` is returned when the command is finished executing, returning the result of the command. It will return a failed result if the command execution failed by errors thrown in the user's own codebase.

It is good to note that every `ICommandResult` implementation overrides `ToString()` with a preformatted message to send to the user. This message can be sent to the user by-for example-calling `consumer.Send(result.ToString())`.

## Handling results

`ResultResolverBase` implementations are used to handle the different types of results. It is possible to define multiple custom result resolvers of your own make.

Like conversion and conditions, to begin writing our own handling logic, we need to implement the abstract type:

```cs
using Commands.Resolvers;

namespace Commands.Samples 
{
    public class MyResultResolver : ResultResolverBase
    {

    }
}
```

Unlike other customizable pipeline components, the `Evaluate` method in the `ResultResolverBase` implementation is not abstract. 
It is possible to filter your implementation by certain results, exclusively failed results:

```cs
using Commands.Resolvers;

namespace Commands.Samples 
{
    public class MyResultResolver : ResultResolverBase
    {
        protected override ValueTask CommandNotFound(ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
        {
		
        }
    }
}
```

The `CommandNotFound` method is called when the search operation returns no commands or modules. By catching it here, we can send a custom response to the user that tried to invoke this command:

```cs
...
    protected override ValueTask CommandNotFound(ConsumerBase consumer, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        consumer.Send("No commands or modules were found with your input.");

        return ValueTask.CompletedTask;
    }
...
```

When you have succesfully constructed the logic for handling the result, it is important to register the resolver in the `CommandBuilder`:

```cs
    ...
        builder.AddResultResolver<MyResultResolver>();
    ...
```

## Complex results

When it is necessary to catch all results, or filter by a more complex condition than the default protected methods, the `Evaluate` method can be overridden.

```cs
using Commands.Resolvers;

namespace Commands.Samples 
{
	public class MyResultResolver : ResultResolverBase
	{
		public override ValueTask Evaluate(ConsumerBase consumer, ICommandResult result, IServiceProvider services, CancellationToken cancellationToken)
		{

		}
	}
}
```

In this method, the `ICommandResult` parameter can be cast to the specific result type, and the logic can be implemented to handle the result accordingly.

## Lightweight implementation

In addition to the broad API, there is also lightweight support for result handling. 
This API is implemented by `CommandBuilder` and will serve most elementary usecases of the functionality.

```cs
using Commands;

var builder = CommandManager.CreateDefaultBuilder();

...

builder.AddResultResolver();
```

When observing this method, there are multiple overloads to use. The one we will use for our lightweight implementation will be the one implementing a delegate.

```cs
builder.AddResultResolver((consumer, result, services) => { });
```

Finally, the delegate will be implemented to handle the result:

```cs
builder.AddResultResolver((consumer, result, services) =>
{
    if (!result.Success)
    {
        consumer.Send(result);
    }
});
```