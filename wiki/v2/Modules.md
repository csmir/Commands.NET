Commands.NET aims to be unrestricting in how commands are defined. 
This means that you can define commands in any way you want, and you can define as many commands as you want.

This article will cover how modules work, which command scenarios are *at least* supported, and how they can be used.

- [Basic usage](#basic-usage)
- [Command overloading](#command-overloading)
- [Class-level execution](#class-commands)
- [Nesting commands](#nesting-commands)
- [Static definitions](#static-definitions)
- [Service injection](#service-injection)

## Basic usage

Every `public` method, instance or static, declared in a `public` class that inherits from `CommandModule` is a command. The `NameAttribute` attribute specifies the name the command is executed by.

```cs
// 'command' is valid
[Name("command")]
public void Command()
{
}
```

A method without a name is the default command of its module, and is executed when the name of the module is provided without a subcommand. 
To prevent a `public` method from becoming a command, mark it with `IgnoreAttribute`.

```cs
[Name("module")]
public class Module : CommandModule
{
    // 'module' is valid
    public void Default()
    {
    }

    // Not a command.
    [Ignore]
    public void Helper()
    {
    }
}
```

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

By using the `Name` attribute on the class itself, all commands in the class are grouped under that name. Methods without a name of their own become default commands of the group, so overloads can be defined by signature alone.

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

Still, it is possible to pass the execution state to the static method without making an instance of the surrounding class. 
For access to this data, the `IContext` implementation used to execute this command can be written directly into the method signature.

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

## Service injection

Commands support service injection. Every parameter of a module constructor is a dependency, resolved from the `IServiceProvider` in `ExecutionOptions` when an instance command is executed. A new instance of the module is created for every execution.

```cs
[Name("command")]
public class CommandClass(MyService myService) : CommandModule
{
    // 'command 1' is valid
    public string Command(int value)
    {
        return myService.DoSomething(value);
    }
}
```

Static and delegate commands, or instance commands that only need a service in a single command, can have services injected into their method signature using the `[Dependency]` attribute. These are resolved on execution.

```cs
[Name("command")]
public class CommandClass : CommandModule
{
    // 'command 1' is valid
    public static void Command(IContext context, [Dependency] MyService myService, int value)
    {
        context.Respond(myService.DoSomething(value));
    }
}
```

The `IServiceProvider` itself and the `IComponentProvider` executing the command can always be injected, even when they are not registered as services. 
When a command is ran directly through `Command.Run`, the `IComponentProvider` is only available when it is registered in the `IServiceProvider`.

> [!NOTE]
> A dependency that cannot be resolved is injected as `null` when it is nullable, or uses its default value when it is optional. Otherwise, the command fails with a `ComponentFormatException`.