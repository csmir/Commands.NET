Command results are handled by implementations of the `IExecuteResult` interface.
When handling results, the different types of results are handled by the `ResultHandler` implementations. This document elaborates on how this process works.

- [Result types](#result-types)
- [Handling results](#handling-results)
- [Complex results](#complex-results)
- [Extended Implementations](#extended-implementations)

## Result types

Amongst the different types of results, the scenario is different for each variant:

> Each result implements a `Success` property, which is used to determine if the result was successful or not. 
If `false`, the `Exception` property will contain the exception that was thrown, never being `null` on failure.

- `SearchResult` is returned in the search operation, containing discovered commands if any. It will return a failed result if:
  - No commands or modules were found.
  - A module was found, but no methods are available to execute.

- `MatchResult` is returned in the match operation, containing the commands that are ready for execution if any. It will return a failed result if:
  - An argument mismatch occurred between the command and the input.
  - A type conversion between the input and the target type of an argument failed.

- `ConditionResult` is returned in the condition evaluation, containing the result of the condition check. It will return a failed result if one or multiple conditions were unmet.

- `InvokeResult` is returned when the command is finished executing, containing the result of the command. It will return a failed result if the command execution failed by errors thrown in the user's own codebase.

It is good to note that every `IExecuteResult` implementation overrides `ToString()` with a preformatted message to send to the user. This message can be sent to the user by-for example-calling `caller.Respond(result)`.

## Handling results

`ResultHandler` implementations are used to handle the different types of results. It is possible to define multiple custom result resolvers of your own make.

Like conversion and conditions, to begin writing our own handling logic, we need to implement the abstract type:

```cs
using Commands;

namespace Commands.Samples;

public class CustomHandler : ResultHandler
{
}

```

Unlike other customizable pipeline components, the `Evaluate` method in the `ResultHandler` implementation is not abstract. 
It is possible to filter your implementation by certain results, exclusively failed results:

```cs
using Commands;

namespace Commands.Samples;

public class CustomHandler : ResultHandler
{
    protected override ValueTask CommandNotFound(ICallerContext caller, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
		
    }
}

```

The `CommandNotFound` method is called when the search operation returns no commands or modules. By catching it here, we can send a custom response to the user that tried to invoke this command:

```cs
...
    protected override ValueTask CommandNotFound(ICallerContext caller, SearchResult result, IServiceProvider services, CancellationToken cancellationToken)
    {
        caller.Send("No commands or modules were found with your input.");

        return ValueTask.CompletedTask;
    }
...
```

When you have succesfully constructed the logic for handling the result, you can pass it along when creating a new `ComponentManager`:

```cs
var manager = ComponentManager.Create([], new CustomHandler());
```

Or, when using the builder pattern:
```cs
builder.AddResultHandler(new CustomHandler());
```

## Complex results

When it is necessary to catch all results, or filter by a more complex condition than the default protected methods, the `HandleResult` method can be overridden.

```cs
using Commands;

namespace Commands.Samples;

public class CustomHandler : ResultHandler
{
	public override ValueTask HandleResult(ICallerContext caller, IExecuteResult result, IServiceProvider services, CancellationToken cancellationToken)
	{

	}
}
```

In this method, the `IExecuteResult` parameter can be cast to the specific result type, and the logic can be implemented to handle the result accordingly.

## Extended Implementations

ResultHandler has a delegate-based implementation which can be used exclusively to handle *faulty* results. This is done by creating a new instance of `DelegateResultHandler`:

```cs
var handler = new DelegateResultHandler((ctx, result, services) => ...);
```