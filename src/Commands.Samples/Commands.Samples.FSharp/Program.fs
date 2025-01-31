open Commands
open Commands.Samples;
open System

let manager = new ComponentCollection()

printf "Added %i components." (manager.AddRange(typeof<FSharpModule>.Assembly.GetExportedTypes()))

while true do
    let input = Console.ReadLine()
    manager.Execute<ConsoleContext>(new ConsoleContext(input)) |> Async.AwaitTask |> Async.RunSynchronously