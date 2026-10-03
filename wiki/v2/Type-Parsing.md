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
using Commands.Parsing;

var parser = new TryParseParser<CustomObject>(CustomObject.TryParse);
```

The functional pattern wraps any `TryParse`-style method or delegate, matching `bool (string? str, out T value)`. 
When the delegate returns `true` the parse succeeds with its out-value, otherwise the parse fails and the command is not executed.

### Declarative Pattern

```cs
using Commands.Parsing;

public class CustomTypeParser : TypeParser<Type>
{
    public override ValueTask<ParseResult> Parse(
        IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        // Your parsing logic here.
    }
}
```

The declarative pattern implements shorthand access to `ParseResult` using the exposed `Error()` and `Success()` methods.

### Attribute Pattern

```cs
using Commands.Parsing;

public class CustomTypeParserAttribute : TypeParserAttribute
{
    public override ValueTask<ParseResult> Parse(
        IContext context, ICommandParameter parameter, object? argument, IServiceProvider services, CancellationToken cancellationToken)
    {
        // Your parsing logic here.
    }
}
```

The attribute pattern writes similar to `ExecuteCondition` implementations, also supporting `Error()` and `Success()` methods for returning results.

Attribute based parsers do not have a generic constraint, because the target type is defined for the parameter they are applied to.

## Applying a Parser

### Declarative

Functional and declarative parsers are registered for their target type:

```cs
ComponentOptions.Default.Parsers[typeof(CustomObject)] = new CustomTypeParser();
```

### Attributes

```cs
[Name("command")]
public void Command([CustomTypeParser] Type type)
{
}
```

Attribute based parsers are applied to the parameter of a command. 
Parameters marked by `DeconstructAttribute` do not support direct parsing, but every non-deconstructed argument within the chosen type constructor does.