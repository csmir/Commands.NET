open Commands
open Commands.Samples;
open System

let provider = new ComponentProvider()

printf "Added %i components." (provider.Components.AddRange(typeof<FSharpModule>.Assembly.GetExportedTypes()))

while true do
    let input = Console.ReadLine()
    provider.Execute<ConsoleContext>(new ConsoleContext(input)) |> Async.AwaitTask |> Async.RunSynchronously