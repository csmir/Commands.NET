The library supports all* argument types. The difference lies in how they are specified, and where.

This article introduces how to use various argument types in commands.

- [Base parameters](#base-parameters)
- [Nullable parameters](#nullable-parameters)
- [Optional parameters](#optional-parameters)
- [Complex parameters](#complex-parameters)
- [Remainder](#remainder)
- [Naming parameters](#naming-parameters)

\* When specifying 'all', that is relative to the implementation of `TypeConverterBase` types for non-standard implementations.
## Base parameters

By default, non-optional parameters are considered a required argument, without any special syntax. They are converted to, from a single input argument.

```cs
// 'command arg1 2 true' is valid
[Name("command")]
public void Command(string arg1, int arg2, bool arg3)
{
    
}
```

> The library will automatically convert the input arguments to the parameter types, if possible. 
If the conversion fails, the command will not be executed and return a `ConvertResult` with an `Exception`.

## Nullable parameters

Nullable parameters are supported, and are considered non-optional arguments that accept `null` as a value.

```cs
// 'command arg1 null true' is valid
[Name("command")]
public void Command(string arg1, int? arg2, bool arg3)
{
    
}
```

## Optional parameters

Optional parameters are supported, and are considered optional arguments that can be omitted. 

```cs
// 'command arg1' is valid
[Name("command")]
public void Command(string arg1, int arg2 = 2, bool arg3 = true)
{
    
}
```

## Complex parameters

Complex parameters are unique in that they are redirected to the signature of a class constructor:

```cs
public class ComplexType
{
    public string Arg1 { get; set; }
    public int Arg2 { get; set; }
    public bool Arg3 { get; set; }
    
    public ComplexType(string arg1, int arg2, bool arg3)
    {
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
    }
}
```

These parameters are considered non-optional, and are specified with the `Complex` attribute.

```cs
// 'command arg1 2 true' is valid
[Name("command")]
public void Command([Complex] ComplexType complex)
{
    
}
```

> Complex parameter constructors are an extension of the commands' own parsing logic. 
In this example, `string`, `int`, and `bool` are added as individual arguments to the command.

## Remainder

When a parameter is marked with the `Remainder` attribute, or with `params`, it will consume all remaining arguments. This is useful for commands that accept a variable number of arguments.

Remainder parameters behave differently depending on the type of the parameter:

- If the parameter is a `string` or `object`, it will consume all remaining arguments as a single string.
- If the parameter is any implementation of `IEnumerable<T>`, it will consume all remaining arguments as an array of `T`.

```cs
// 'command 1 2 3 4 5 6' is valid
[Name("command")]
public void Command(params int[] args)
{
    
}
```

## Naming parameters

Parameters can be named by using the `Name` attribute. This is useful for commands that have camel-case parameter names, but should have lowercase naming to the consumer.

```cs
// 'command arg1 2 arg3' is valid
[Name("command")]
public void Command([Name("arg1")] string Arg1, [Name("arg2")] int Arg2, [Name("arg3")] bool Arg3)
{
    
}
```