Every command can have a different return type, from another. In order to handle the possible scenarios in which a developer might find themselves, the library tries to resolve as many as possible.

- [Basic return types](#basic-return-types)

## Basic return types

Amongst the basic return types, the library supports:

- `void`
- `object`
- `string`
- `T`
- `Task`
- `Task<T>`
- `ValueTask`
- `ValueTask<T>`

When returning `void`, the library will not send a response to the consumer.

```cs
// 'void' is valid
[Name("void")]
public void GetVoid()
{
    
}
```

When returning `string`, `T` or `object` the library will send the message to the consumer.

```cs
// 'string' is valid
[Name("string")]
public string GetString()
{
    return "string";
}
```

```cs
// 'object' is valid
[Name("object")]
public object GetObject()
{
    return new();
}
```

When returning `Task` or `ValueTask`, the library will await the task. If the task returns a value, it *will* be sent to the consumer. If there is no value, the library will not send a response.

```cs
// 'task' is valid
[Name("task")]
public Task GetTask()
{
    return Task.CompletedTask;
}
```

> The library will automatically convert the return type to a string, if it is `T` and unhandled.
