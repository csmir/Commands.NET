Command results are handled by implementations of the `IExecuteResult` interface.
When handling results, the different types of results are handled by the `ResultHandler` implementations. This document elaborates on how this process works.

- [Result Types](#result-types)
- [Handling Results](#handling-results)

## Result types

There are four different types of results that can be returned from the command pipeline. Between these results, the `IExecuteResult` interface is implemented. 

> [!NOTE] Each result implements a `Success` property, which is used to determine if the result was successful or not. 
> If `false`, the `Exception` property will contain the exception that was thrown.

### SearchResult

`SearchResult` is returned when the discovery operation failed to yield result.

When this result is received, the `Exception` property can contain one of two exceptions:

- `CommandNotFoundException`: No commands or groups were found.
- `CommandRouteIncompleteException`: A group was found, but no methods are available to execute.

### ParseResult

`ParseResult` is returned from the parsing operation when the input could not be parsed into command parameters.

When this result is received, the `Exception` property can contain one of two exceptions:

- `CommandOutOfRangeException`: The provided arguments are out of range of the command.
- `CommandParsingException`: A type conversion between the input and the target type of an argument failed. This exception holds the result of the first failed conversion.

### ConditionResult

`ConditionResult` is returned after condition evaluation if one or multiple conditions were unmet.

Its `Exception` property contains a `CommandEvaluationException` which holds the result of the first failed condition.

### InvokeResult

`InvokeResult` is returned when the command is finished executing, containing the result of the command. 
It will return a failed result if the command execution failed by errors thrown in the user's own codebase.

> [!TIP] It is good to note that every `IExecuteResult` implementation overrides `ToString()` with a preformatted message to send to the user. 
> This message can be sent to the user by-for example-calling `caller.Respond(result)`.

## Handling Results

`ResultHandler` implementations are used to handle the different types of results. It is possible to define multiple custom handlers of your own make.

### Delegate Implementations

ResultHandler has a delegate-based implementation which can be used exclusively to handle *faulty* results:

```cs
var handler = ResultHandler.Create<TContext>((ctx, result, services) => ...);
```

Here, `TContext` is the `ICallerContext` implementation that this handler will handle. 
If the context is not of a matching type, this handler will not be called.

### Class-based Implementations

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

public class CustomResultHandler : ResultHandler
{
    protected override ValueTask CommandNotFound(ICallerContext caller, SearchResult result, string errorReason, IServiceProvider services, CancellationToken cancellationToken)
    {
        // Your response logic here
    }
}

```

The `CommandNotFound` method is called when the search operation returns no commands or modules.

When you have succesfully constructed the logic for handling the result, you can pass it along when creating a new `ComponentManager`:

```cs
var manager = ComponentManager.From().Handler(new CustomResultHandler());
```

> [!NOTE]
> `ResultHandler<TContext>` can be used to filter by a specific context type. This is useful when you want to handle results differently depending on the context.