A `TypeParser` reads provided argument input in string format and try to convert them into the types as defined in the command signature.
Let's work on an example to learn how they work.

- [Default Parsers](#default-parsers)
- [Creating a Parser](#creating-a-parser)
- [Applying a Parser](#applying-a-parser)

## Default Parsers

By default, the library provides parsers for the following types, without any additional configuration:

- All primitive [BCL](https://learn.microsoft.com/en-us/dotnet/standard/class-library-overview#system-namespace) types.
- Implementations of `Enum`.
- [BCL](https://learn.microsoft.com/en-us/dotnet/standard/class-library-overview#system-namespace) types contained within `Array`
- Commonly used structs, being: 
    - `DateTime`
    - `DateTimeOffset`
    - `TimeSpan`
    - `Guid`
    - `Color`

In addition to creating a custom parser for unsupported types, you can also override these default parsers by registering your own parser for the type. 
Any user-defined parsers will take precedence over the default parsers.

## Creating a Parser

### Functional Pattern

```cs
var parser = TypeParser.For<Type>().AddDelegate((ctx, param, value, services) => ...);
```

The creation pattern handles conditions as `ValueTask<ParseResult>` where `ParseResult.FromError()` or `ParseResult.FromSuccess()` can be used to return the result. 
`ParseResult` implicitly converts to `ValueTask<T>`.

### Declarative Pattern

```cs
using Commands.Parsing;

public class CustomTypeParser : TypeParser<Type>
{
    public override ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        // Your parsing logic here.
    }
}
```

The declarative pattern implements shorthand access to `ParseResult` using the exposed `Error()` and `Success()` methods.

### Attribute Pattern

```cs
using Commands.Parsing;

public class CustomTypeParserAttribute : TypeParserAttribute<Type>
{
    public override ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        // Your parsing logic here.
    }
}
```

The attribute pattern writes similar to `ExecuteCondition` implementations, also allowing shorthand calls to be used.

## Applying a Parser

### Functional Pattern & Declarative Pattern

```cs
ComponentConfiguration.From(...).AddParser(parser);
```
```cs
ComponentConfiguration.From(...).AddParser(new CustomTypeParser());
```

### Attribute Pattern

```cs
[Name("command")]
public void Command([CustomTypeParser] Type type)
{
}
```

Attribute based parsers are applied to the parameter of a command. 
Parameters marked by `DeconstructAttribute` do not support parsing, but every non-deconstructed argument within the applicable signature does.