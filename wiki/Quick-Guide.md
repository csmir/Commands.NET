This guide introduces you to Commands.NET, covering the basics for developers ranging from beginner to veteran.

- [Before Coding](#before-coding)
- [Defining Commands](#defining-commands)
- [Running your Commands](#running-your-commands)
- [Putting it Together](#summary)

## Before Coding

Before we can start writing code, there are a few things to set up. This includes getting a fresh project to start, and installing Commands.NET.

### Creating a New Project

Before being able to write code, we need to give ourselves a place to do it. 
With your favourite code editor, we will create a new empty project. 
This project can be anything, but in our case, a **Console Application** will be the focus. 
If you are using `dotnet-cli`, you can create such a project by executing `dotnet new console`. 
Keep in mind that the project has to target `.net8` or higher.

> In a visual editor such as Rider or Visual studio, you can create it by simply opening the application, navigating to 'Create New Project' and selecting the Console Application template.

### Installing the Package

To be able to reference Commands.NET, we will need to head over to your package manager to install the package. 
This can be in the PM console, in your `.csproj` file or through the code editor.
We can grab the latest version from [NuGet](https://www.nuget.org/packages/Commands.NET). 
If you prefer to use the visual tools, simply look up `commands.net` and install the first package available.

## Defining Commands

### Creating a Module

Let's create a new file in your favourite code editor. 
The ideal way to do this is by calling it `XModule.cs` where `X` is its name or category.

After creating the file, we should see the following:

```cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands.Samples
{
    internal class ExampleModule
    {
    }
}
```

Next up, we will want to make the class public, and turn it into a module. 
To do that, we need to include `Commands.Core` into the usings, and change the signature of the class to inherit `ModuleBase`. 
With both of those steps out of the way, it should now look like this:

```cs
using Commands;

namespace Commands.Samples
{
    public class ExampleModule : ModuleBase
    {
    }
}
```

> Starting in .NET6, [implicit usings](https://learn.microsoft.com/en-gb/dotnet/core/project-sdk/overview#implicit-using-directives) are part of the core features. 
> Because we will assume that this is a clean, newly created project on .NET 6 or higher, we can remove the usings without an effect on the functionality. 
> If you do see errors from removing the extra usings, please ignore this advice.

### Creating Commands

With your basic module complete, we are now ready to start writing commands. 
To begin, we will write 2 commands:

#### Command 1: Hello World

Within the class declaration of the file, we will write a new method. 
The method can return `Task` , `ValueTask` or `void`, but for the sake of this guide, we will use `void`.

```cs
using Commands;

namespace Commands.Samples
{
    public class ExampleModule : ModuleBase
    {
        public void HelloWorld()
        {
        }
    }
}
```

Next up, we will want to decorate the method with an attribute. The `[Name]` attribute accepts a name, and registers the command by it, so we can use that name to run it from our input source. We will call the command `helloworld`. Let's add the attribute and see how that looks:

```cs
using Commands;

namespace Commands.Samples
{
    public class ExampleModule : ModuleBase
    {
        [Name("helloworld")]
        public void HelloWorld()
        {
        }
    }
}
```

Now the `HelloWorld` method will run when we run the `helloworld` command, but there is still one thing missing. The method is empty! You can add anything here, but in our case, a simple reply will do:

```cs
using Commands;

namespace Commands.Samples
{
    public class ExampleModule : ModuleBase
    {
        [Name("helloworld")]
        public void HelloWorld()
        {
            Console.WriteLine("Hello world!");
        }
    }
}
```

> To actually start testing and running the command, skip to [here](#-running-your-commands).

#### Command 2: Reply

Our next command will be quite simple as well. Just like before, we can create a new method, mark it with an attribute and give it a name. Except this time, it will accept input, and respond with it. Let's create this functionality:

```cs
using Commands;

namespace Commands.Samples
{
    public class ExampleModule : ModuleBase
    {
        [Name("helloworld")]
        public void HelloWorld()
        {
            Respond("Hello world!");
        }

        [Name("reply")]
        public void Reply([Remainder] string message)
        {
            Respond(message);
        }
    }
}
```

As you can see here, the reply method accepts a parameter called `message`, and replies to the command with its value. 
Normally, this would automatically accept a string parameter, usually a single word: `word` or multiple words connected by quotations: `"a few words"`.

In our case however, the command is marked with `[Remainder]`. 
This attribute has a special property, it ignores all parameters provided after it. Instead, it connects all of them together and makes it one long string (or object). 
This way you don't need to add quotation marks, making the command more intuitive to use.

## Running your Commands

With the module made, we need to create our means to actually find and run the commands inside of it. 
Most of the information to do so, is already handled internally. However, we need to define the basics still.
For this, we will go back to our `Program.cs` file:

```cs
using Commands;
using Commands.Parsing;
using Commands.Samples;

var builder = CommandManager.CreateBuilder();
```

The `CreateBuilder()` method creates a `CommandBuilder<T>` for us to use. 
With this builder, we are free to configure some of the essential options to receive and bring in information about registration and execution of commands.

If the command failed in any way, an `ICommandResult` brought forward by the execution will cover where it went wrong and contain the exception that occurred. 
Here, it is simplest to see if it failed, and then throw the exception if it did. For this to be handled the way we expect it to, we need to configure it:

```cs
...
builder.AddResultResolver((consumer, result, services) => 
{
    if (!result.Success)
    {
        Console.WriteLine(result);
    }
});
...
```

With that out of the way, we can build the builder into the manager. 
To do this, we call the `Build()` method.

```cs
...
var manager = builder.Build();
...
```

Hovering over the `CommandManager` declaration, you can read some things about it. It is the root type, or class, that makes Commands.NET work as a whole. 
You can greatly customize it and make a lot of changes to your personal taste. 
Though, we only need very little from it for this guide.

### Listening to Input

The most important part of running a command is defining *what* command you want to run. 
In our case, we want to test commands with console input, so we will use `Console.ReadLine` to receive inputs.
You can replace the way to get your input with anything you like, ranging from reading a game chat to listening for a discord message.

```cs
...
var parser = new StringParser();

while (true)
{
    var input = parser.Parse(Console.ReadLine());

    var consumer = new CommandBase();
}
...
```

With this input, we can create the most important component to running a command: The consumer. 
Consumers are like users, to which the command is targetted. 
It is exposed during the entire pipeline, holding data about itself and being able to expose methods to be responded to.

### Executing the Command

With the context defined, it is time to run the command. 
By calling `TryExecuteAsync` in the `while` loop, Commands.NET will read your command input and try to find the most appropriate command to run. 
If the input you write in your console matches the signature of your `helloworld` or `reply` command, it will succeed and execute the method that implements it.

```cs
    ...
    framework.TryExecute(context, input);
    ...
```

## Summary

Let's review what we achieved in this guide. 
First of all, we created an empty project and installed the package.
After that, we defined our module that implements 2 commands.
One accepting a value, and one being a simple trigger.

To run the command, we moved back to the `Program.cs` file and implemented the code to set up the commands. 
Finally, we set up the requirements to run it.

Let's take a look at the code we wrote to summarize our work.

### Module and Commands

```cs
#Module.cs
using Commands;

namespace Commands.Samples
{
    public class Module : ModuleBase
    {
        [Command("helloworld")]
        public void HelloWorld()
        {
            Respond("Hello world!");
        }

        [Command("reply")]
        public void Reply([Remainder] string message)
        {
            Respond(message);
        }
    }
}
```

### Setup and Execution

```cs
#Program.cs
using Commands;
using Commands.Parsing;
using Commands.Samples;

var builder = CommandManager.CreateBuilder();

builder.AddResultResolver((consumer, result, services) =>
{
    if (!result.Success)
    {
        Console.WriteLine(result);
    }
});

var framework = builder.Build();

while (true)
{
    var input = StringParser.Parse(Console.ReadLine()!);

    var consumer = new ConsumerBase();

    await framework.TryExecuteAsync(consumer, input);
}
```

If these files match yours, you're good to go. 
To run your command, simply run the application and see the result. 