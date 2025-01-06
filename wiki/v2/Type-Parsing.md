A `TypeParser` reads provided argument input in string format and try to convert them into the types as defined in the command signature.
Let's work on an example to learn how they work.

- [Default Parsers](#default-parsers)
- [Creating a Parser](#creating-your-parser)
- [Using Parsers](#using-parser)
- [Extended Implementations](#extended-implementations)

## Default Parsers

By default, the library provides parsers for the following types, without any additional configuration:

- All primitive BCL types.
- Implementations of `Enum`.
- BCL types contained within `Array`
- Commonly used structs, being: 
    - `DateTime`
    - `DateTimeOffset`
    - `TimeSpan`
    - `Guid`
    - `Color`

In addition to creating a custom parser for unsupported types, you can also override these default parsers by registering your own parser for the type. 
Any user-defined parsers will take precedence over the default parsers.

## Creating a Parser

All TypeParsers inherit `TypeParser<T>` or `TypeParser`. To start creating your parser, you have to inherit one of the two on a class.

> For the simplicity of this documentation, only the generic type is introduced here.

```cs
using Commands;
using Commands.Conversion;

namespace Commands.Samples;

public class SystemTypeParser : TypeParser<Type>
{
    public override ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
    }
}

```

With this class defined and the method that will operate the evaluation being implemented, we can now write our code which defines the succession and failure conditions. 
In case of success, we also need to pass in the parsed object that the TypeParser expects to see returned.

```cs
...
    public override ValueTask<ParseResult> Parse(
        ICallerContext caller, ICommandParameter argument, object? value, IServiceProvider services, CancellationToken cancellationToken)
    {
        try
        {
            var typeSrc = Type.GetType(
                typeName: value?.ToString() ?? "",
                throwOnError: true,
                ignoreCase: true);

            return Success(typeSrc);
        }
        catch (Exception ex)
        {
            return Error($"A type with name '{value}' was not found within the current assembly. Did you provide the type's full name, including its namespace?");
        }
    }
...
```

With the logic defined, we can also add options in the parser, for example by customizing how `ignoreCase` is configured in the `Type` search:

```cs
...
public class SystemTypeParser(bool caseIgnore) : TypeParser<Type>
{
    private readonly bool _caseIgnore = caseIgnore;
        
    ...
}
...
```

```cs

    ...
        var typeSrc = Type.GetType(
            typeName: value, 
            throwOnError: true, 
            ignoreCase: _caseIgnore);
    ...
```

## Using Parsers

There is a variety of ways to configure your commands to use custom parsers. 
Our focus will be on the `CommandConfiguration` class, which is the central point of configuration for the build pipeline.

There are two ways to include a custom parser in the configuration:
```cs
var configuration = ComponentConfiguration.Create(new SystemTypeParser(true));
```
```cs
var configuration = ComponentConfiguration.CreateBuilder().AddParser(new SystemTypeParser(true)).Build();
```

With this configuration, API's that consume its instance will now have access to the custom parser. One example, is creating a component with a normally unsupported type, and use the customized configuration to support it.
```cs
var command = Command.Create((Type type) => type.Name, ["check-type"], configuration);
```

Another example, is browsing the configuration for all available components, including all commands that require the custom parser.
If there is a command that requires the parser, and it is not defined, this method will throw an exception.
```cs
var componentsInAssembly = configuration.CreateComponents(Assembly.GetExecutingAssembly().GetExportedTypes());
```

Lastly, you can also create a module from its type, and use the customized configuration to support defined commands that implement `System.Type`.
```cs
var module = CommandGroup.Create<T>(configuration);
```

## Extended Implementations

The library provides a few types which simplify the implementation of custom parsing logic. One of these, is `DelegateParser<T>`:

```cs
var parser = new DelegateParser<Assembly>((ctx, param, value, services) => ...);
```

Additionally, a `TryParseParser` is available for types that implement `TryParse`:

```cs
var parser = new TryParseParser<int>(int.TryParse);
```