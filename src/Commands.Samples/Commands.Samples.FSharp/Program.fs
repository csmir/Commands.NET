open Commands
open Commands.Samples;
open System

// This sample demonstrates how to use the F# module system with the Commands library. It is a demonstrative example, not necessarily functionality-rich.
// For functionality demonstrations, it is recommended to use the C# samples.

// Create a new provider to execute commands with.
let provider = new ComponentProvider()

// Add the F# module components to the provider.
printf "Added %i components." (provider.Components.AddRange(typeof<FSharpModule>.Assembly.GetExportedTypes()))

// Define a loop to read input from the console and execute commands.
while true do
    let input = Console.ReadLine()
    provider.Execute<ConsoleContext>(new ConsoleContext(input)) |> Async.AwaitTask |> Async.RunSynchronously