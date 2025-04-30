open Commands
open Commands.Samples;
open System

let components = new ComponentCollection()

printf "Added %i components." (components.AddRange(typeof<FSharpModule>.Assembly.GetExportedTypes()))

while true do
    let input = Console.ReadLine()
    components.Execute<ConsoleCallerContext>(new ConsoleCallerContext(input)) |> Async.AwaitTask |> Async.RunSynchronously