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
new Command((string arg1, int arg2, bool arg3) => { }, "name");
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
new Command((string arg1, int? arg2, bool arg3) => { }, "name");
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
new Command((string arg1, int arg2 = 2, bool arg3 = true) => { }, "name");
```
```cs
// 'command arg1' is valid
[Name("command")]
public void Command(string arg1, int arg2 = 2, bool arg3 = true)
{
}
```

## Deconstructible Parameters

Deconstructible parameters are unique in that they are redirected to the signature of a type constructor:

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
new Command(([Deconstruct] ConstructedType complex) => { }, "name");
```
```cs
// 'command arg1 2 true' is valid
[Name("command")]
public void Command([Deconstruct] ConstructedType complex)
{
}
```

Deconstructible parameter constructors are an extension of the commands' own parsing logic. 
In this example, `string`, `int`, and `bool` are added as individual parameters to the command.

> [!NOTE]
> When the target type does not have a public constructor containing any parameters, a `ComponentFormatException` will be thrown on creation.
> It is possible to define which constructor is used by specifying `IgnoreAttribute` on other constructors.

## Remainder

When a parameter is marked with `RemainderAttribute`, an attribute implementing `IRemainderBinding` or with `params`, it will consume all remaining arguments. 
This is useful for commands that accept a variable number of arguments.

Remainder parameters behave differently depending on the type of the parameter:

- If the parameter is a `string`, `object` or `T`, it will consume all remaining arguments as a single string.
- If the parameter is any implementation of `Array`, it will consume all remaining arguments as an array of `T`.

```cs
new Command((string arg1, [Remainder] string arg2) => { }, "name");
```
```cs
// 'command 1 2 3 4 5 6' is valid
[Name("command")]
public void Command(params int[] args)
{
}
```

> [!NOTE]
> A `IRemainderBinding` attribute can only occur on the last parameter of the command signature.
> If it occurs on any other parameter, a `ComponentFormatException` will be thrown on creation.

## Resources

In some cases it is functionally preferred to have a resource, not necessarily part of the command query, to be injected into the command signature as a parameter. 
When the `IContext` used for execution implements `IResourceContext`, 
the `GetResource` method will be used to retrieve a resource bound to the state of the command execution into the parameter marked with `ResourceAttribute` or any other attribute marked with `IResourceBinding`.

```cs
new Command(([Resource] T resource) => { }, "name");
```
```cs
// 'command' is valid when context is `IResourceContext` returning a resource of type `T`
[Name("command")]
public void Command([Resource] T resource)
{
}
```

`T` in this context is a parsible type. The value returned from `GetResource` will be parsed using the `TypeParser` registered for the type `T`.

> [!NOTE]
> Injecting a resource into the command signature is exclusive, and can only be done once. 
> If there are multiple occurrences of `IResourceBinding` on the command signature, a `ComponentFormatException` will be thrown on creation.

## Naming Parameters

Parameters can be named by using the `NameAttribute` or another custom attribute implementing `INameBinding`. 
This is useful for commands that have camel-case parameter names, but should have lowercase naming to the caller.

```cs
new Command(([Name("arg1")] string Arg1, [Name("arg2")] int Arg2, [Name("arg3")] bool Arg3) => { }, "name");
```
```cs
// 'command arg1 2 arg3' is valid
[Name("command")]
public void Command([Name("arg1")] string Arg1, [Name("arg2")] int Arg2, [Name("arg3")] bool Arg3)
{   
}
```