A `TypeConverter` reads provided argument input in string format and try to convert them into the types as defined in the command signature.
Let's work on an example to learn how they work.

- [Creating your TypeConverter](#creating-your-typeconverter)
- [Using your TypeConverter](#using-your-typeconverter)
- [Lightweight implementation](#lightweight-implementation)

## Creating your TypeConverter

All TypeConverter inherit `TypeConverterBase<T>` or `TypeConverterBase`. To start creating your TypeConverter, you have to inherit one of the two on a class.

> For the simplicity of this documentation, only the generic type is introduced here.

```cs
using Commands;
using Commands.Reflection;
using Commands.TypeConverters;

namespace Commands.Samples
{
    public class ReflectionTypeConverter : TypeConverterBase<Type>
    {
        public override ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, string? value, IServiceProvider services, CancellationToken cancellationToken)
        {

        }
    }
}
```

With this class defined and the method that will operate the evaluation being implemented, we can now write our code which defines the succession and failure conditions. In case of success, we also need to pass in the parsed object that the TypeConverter expects to see returned.

```cs
    ...
        public override ValueTask<ConvertResult> Evaluate(ConsumerBase consumer, IArgument argument, string? value, IServiceProvider services, CancellationToken cancellationToken)
        {
            try
            {
                var typeSrc = Type.GetType(
                    typeName: value, 
                    throwOnError: true, 
                    ignoreCase: true);

                return ValueTask.FromResult(Success(typeSrc));
            }
            catch (Exception ex)
            {
                return ValueTask.FromResult(Error(ex));
            }
        }
    ...
```

With the logic defined, we can also add options in the converter, for example by customizing how `ignoreCase` is configured in the `Type` search:

```cs
...
    public class ReflectionTypeConverter(bool caseIgnore) : TypeConverterBase<Type>
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

## Using your TypeConverter

After you have written your TypeConverter, it is time to use it. Let's define a command that receives a `Type` as one of its parameters.

```cs
    ...
        [Command("type-info", "typeinfo", "type")]
        public void TypeInfo(Type type)
        {
            Console.WriteLine($"Information about: {type.Name}");

            Console.WriteLine($"Fullname: {type.FullName}");
            Console.WriteLine($"Assembly: {type.Assembly.FullName}");
        }
    ...
```

If you start the program now, it will throw an error on startup. `System.Type` has no known `TypeConverter`. In order to resolve this error, we must return to the `Program.cs` file of your application, and define the `TypeConverter` through configuration:

```cs
    ...
        builder.AddTypeConverter(new ReflectionTypeConverter(caseIgnore: true));
    ...
```

Restarting the program now, you can try out your new command and see the results for yourself.


## Lightweight implementation

In addition to the broad API, there is also lightweight support for type conversion. 
This API is implemented by `CommandBuilder` and will serve most elementary usecases of the functionality.

To make use of this API, we have to take a look at how the `CommandManager` is configured in the `Program.cs` file of your application. 
The `CommandBuilder` is used to configure the `CommandManager` and is used to add `TypeConverter`'s to the pipeline.

```cs
using Commands;

var builder = CommandManager.CreateDefaultBuilder();

...

builder.AddTypeConverter<Version>();
```

When observing this method, there are multiple overloads to use. 
The one we will use for the `Version` conversion, is going to be the one implementing a `Func` that returns a conversion result:

```cs
builder.AddTypeConverter<Version>((consumer, argument, value, services) => { });
```

Finally, the `Func` will be implemented to convert the string input into a `Version` object:

```cs
builder.AddTypeConverter<Version>((consumer, argument, value, services) =>
{
    if (Version.TryParse(value, out var version))
    {
        return ConvertResult.FromSuccess(version);
    }

    return ConvertResult.FromError(new FormatException("Invalid version format."));
});
```