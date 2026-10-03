Every command can have a different return type, from another. 
In order to handle the possible scenarios in which a developer might find themselves, the library tries to resolve as many as possible.

- [Basic Return Types](#basic-return-types)
- [Custom Return Type Handling](#custom-return-type-handling)

## Basic Return Types

Amongst basic return types, the library supports:

- `void`
- `object`
- `string`
- `T : notnull`
- `Task`
- `Task<T : notnull>`
- `ValueTask`
- `ValueTask<T : notnull>`

When returning `void`, the library will not send a response to the caller.

```cs
new Command(() => { }, "void");
```
```cs
// 'void' is valid
[Name("void")]
public void GetVoid()
{
}
```

When returning `string`, `T` or `object` the library will send the return value to the caller.

```cs
new Command(() => "string", "string");
```
```cs
// 'string' is valid
[Name("string")]
public string GetString()
{
    return "string";
}
```

```cs
new Command(() => new object(), "object");
```
```cs
// 'object' is valid
[Name("object")]
public object GetObject()
{
    return new();
}
```

When returning `Task` or `ValueTask`, the library will await the task. When returning `Task<T>` or `ValueTask<T>`, the result of the task *will* be sent to the caller. If there is no value, the library will not send a response.

```cs
new Command(() => Task.CompletedTask, "task");
```
```cs
// 'task' is valid
[Name("task")]
public Task GetTask()
{
    return Task.CompletedTask;
}
```

The command is considered finished when its task completes. Exceptions thrown by an asynchronous command, before or after awaiting, fail the command and are passed to `OnFailure`, just like exceptions thrown synchronously.

> [!NOTE]
> How a return value is handled is decided by the *declared* return type of the command. A command declared to return `object` that returns a `Task` will send the task itself, rather than awaiting it.
> Furthermore, the library will convert the return type to a `string` by calling `ToString`, if it is `T` and unhandled. 
> If the consumer desires to display `T` in a different way, they can override `ToString` in the class.

`CommandReturnType` exposes this handling, to await and unwrap the return value of a method yourself:

```cs
var returnType = new CommandReturnType(method);

var result = await returnType.GetAsyncResult(method.Invoke(instance, args));
```

## Custom Return Type Handling

It is possible to implement the handling of custom return types by implementing `ComponentProvider.Finalize` in a derived class.

```cs
using Commands;

public class CustomComponentProvider : ComponentProvider
{
    protected override Task Finalize<TContext>(TContext context, IResult result, ExecutionOptions options)
        where TContext : class, IContext
    {
        // Do work.
    }
}
```

> [!TIP]
> Default return type handling, `OnFailure` and `OnSuccess` are invoked by the base implementation, so it is recommended to call `base.Finalize` after custom logic is executed.