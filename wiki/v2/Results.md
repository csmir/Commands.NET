Command results are handled by implementations of the `IResult` interface.
When handling results, events can be implemented within the `IComponentProvider`. This document elaborates on how this process works.

- [Result Types](#result-types)
- [Handling Results](#handling-results)

## Result types

There are four different types of results that can be returned from the command pipeline. Between these results, the `IResult` interface is implemented. 

> [!NOTE] 
> Each result implements a `Success` property, which is used to determine if the result was successful or not. 
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

> [!TIP] 
> It is good to note that every `IResult` implementation overrides `ToString()` with a preformatted message to send to the user. 
> This message can be sent to the user by-for example-calling `caller.Respond(result)`.

## Handling Results

Results can be handled by implementing one of two events exposed by the `IComponentProvider`: `OnSuccess` and `OnFailure`.

These events are invoked when a result is returned from the command pipeline, and can be implemented as follows:

```cs
using Commands;

var provider = new ComponentProvider();

provider.OnSuccess += (context, result, services) => 
{
	// Logic
};

provider.OnFailure += (context, result, exception, services) => 
{
	// Logic
};
```