Commands.NET aims to be unrestricting in how commands are defined. 
This means that you can define commands in any way you want, and you can define as many commands as you want.

This article will cover how modules work, which command scenarios are *at least* supported, and how they can be used.

- [Basic usage](#basic-usage)
- [Command overloading](#command-overloading)
- [Class-level execution](#class-commands)
- [Nesting commands](#nesting-commands)
- [Static definitions](#static-definitions)

## Basic usage

Commands in modules are defined by creating a method with the `NameAttribute` attribute. The `NameAttribute` attribute specifies the name of the command that the method will be executed.

```cs
// 'command' is valid
[Name("command")]
public void Command()
{
}
```

Only `public` methods can be commands, and they must be in a class that is `public` and inherits from `CommandModule`.

## Command overloading

Commands can be overloaded by defining multiple methods with the same name. The library will automatically select the method with the most matching arguments.

```cs
// 'command 1' is valid
[Name("command")]
public void Command(int arg1)
{
}

// 'command 1 2' is valid
[Name("command")]
public void Command(int arg1, int arg2)
{
}
```

> [!IMPORTANT] 
> Overloads are executed in order of score. Score is calculated by length of a signature and the importance of each argument. 
> The `GetScore` method in `Command` reveals the score of a signature.

## Class commands

Commands can be defined in a class by using the `Name` attribute on the class itself. This will make all methods in the class commands.

```cs
[Name("command")]
public class CommandClass : CommandModule
{
    // 'command arg1' is valid
    public void Command(string arg1)
    {
    }
    
    // 'command arg1 2' is valid
    public void Command(string arg1, int arg2)
    {
    }
}
```

> [!TIP]
> By specifying `Ignore` on a method, it will not be considered a command. 
> When it is specified on a class, the whole class will be ignored, including nested classes.

## Nesting commands

Commands can be nested by defining a class with the `Name` attribute, and then defining methods in that class with the `Name` attribute.

```cs
[Name("command")]
public class CommandClass : CommandModule
{
    // 'command subcommand' is valid
    [Name("subcommand")]
    public void SubCommand()
    {
    }
}
```

Additionally, nested classes can also be specified to create further nesting.

```cs
[Name("command")]
public class CommandClass : CommandModule
{
    [Name("subcommand")]
    public class SubCommandClass : CommandModule
    {
        // 'command subcommand subsubcommand' is valid
        [Name("subsubcommand")]
        public void SubSubCommand()
        {
        }
    }
}
```

## Static definitions

When it is not necessary to use the instance of a class to execute a command, the method can be defined as `static` and execute statelessly.

```cs
[Name("command")]
public class CommandClass : CommandModule
{
    // 'command' is valid
    public static void Command()
    {
    }
}
```

Even still, it is possible to pass the execution state to the static method without making an instance of the surrounding class. 
For access to this data, the `IContext` implementation used to execute this command can be injected directly into the method.

```cs
[Name("command")]
public class CommandClass : CommandModule
{
    // 'command 1' is valid
    public static void Command(IContext context, int value)
    {
        context.Respond(value);
    }
}
```

Static (and delegate) commands also support service injection. 
All services that are registered in the `IServiceProvider` can be injected into the method, by specifying them as parameters preceding the `IContext` parameter.

```cs
[Name("command")]
public class CommandClass : CommandModule
{
    // 'command 1' is valid
    public static void Command(IServiceProvider services, IContext context, int value)
    {
        var myService = services.GetService<MyService>();
        context.Respond(myService.DoSomething(value));
    }
}
```