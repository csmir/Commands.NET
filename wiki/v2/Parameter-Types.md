The library supports all* parameter types. The difference lies in how they are specified, and where.

This article introduces how to use various parameter types in commands.

- [Base Parameters](#base-parameters)
- [Nullable Parameters](#nullable-parameters)
- [Optional Parameters](#optional-parameters)
- [Deconstructible Parameters](#complex-parameters)
- [Remainder](#remainder)
- [Naming Parameters](#naming-parameters)

\* When specifying 'all', that is relative to the implementation of `TypeParser` types for non-standard implementations.

## Base Parameters

By default, non-optional parameters are considered a required argument, without any special syntax. They are converted to, from a single input argument.

```cs
Command.Create((string arg1, int arg2, bool arg3) => { });
```
```cs
// 'command arg1 2 true' is valid
[Name("command")]
public void Command(string arg1, int arg2, bool arg3)
{
}
```

> [!IMPORTANT] 
> The library will automatically convert the input arguments to the parameter types, if possible. 
> If the conversion fails, the command will not be executed and return a `MatchResult` with an `Exception`.

## Nullable Parameters

Nullable parameters are supported, and are considered non-optional arguments that accept `null` as a value.

```cs
Command.Create((string arg1, int? arg2, bool arg3) => { });
```
```cs
// 'command arg1 null true' is valid
[Name("command")]
public void Command(string arg1, int? arg2, bool arg3)
{
}
```

## Optional Parameters

Optional parameters are supported, and are considered optional arguments that can be omitted. 

```cs
Command.Create((string arg1, int arg2 = 2, bool arg3 = true) => { });
```
```cs
// 'command arg1' is valid
[Name("command")]
public void Command(string arg1, int arg2 = 2, bool arg3 = true)
{
}
```

## Deconstructible Parameters

Deconstructible parameters are unique in that they are redirected to the signature of a class constructor:

```cs
public class ConstructedType
{
    public string Arg1 { get; set; }
    public int Arg2 { get; set; }
    public bool Arg3 { get; set; }
    
    public ConstructedType(string arg1, int arg2, bool arg3)
    {
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
    }
}
```

These parameters are specified with the `Deconstruct` attribute.

```cs
Command.Create(([Deconstruct] ConstructedType complex) => { });
```
```cs
// 'command arg1 2 true' is valid
[Name("command")]
public void Command([Deconstruct] ConstructedType complex)
{
}
```

> [!NOTE]
> Deconstructible parameter constructors are an extension of the commands' own parsing logic. 
> In this example, `string`, `int`, and `bool` are added as individual arguments to the command.

## Remainder

When a parameter is marked with the `Remainder` attribute, or with `params`, it will consume all remaining arguments. This is useful for commands that accept a variable number of arguments.

Remainder parameters behave differently depending on the type of the parameter:

- If the parameter is a `string`, `object` or `T`, it will consume all remaining arguments as a single string.
- If the parameter is any implementation of `Array`, it will consume all remaining arguments as an array of `T`.

```cs
Command.Create((string arg1, [Remainder] string arg2) => { });
```
```cs
// 'command 1 2 3 4 5 6' is valid
[Name("command")]
public void Command(params int[] args)
{
}
```

## Naming Parameters

Parameters can be named by using the `Name` attribute. This is useful for commands that have camel-case parameter names, but should have lowercase naming to the caller.

```cs
Command.Create(([Name("arg1")] string Arg1, [Name("arg2")] int Arg2, [Name("arg3")] bool Arg3) => { });
```
```cs
// 'command arg1 2 arg3' is valid
[Name("command")]
public void Command([Name("arg1")] string Arg1, [Name("arg2")] int Arg2, [Name("arg3")] bool Arg3)
{   
}
```