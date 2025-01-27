open Commands
open System

let command = new Command(Func<_>(fun () -> "Hello world"), "hello")

let manager = new ComponentManager() 
[ command ] |> Seq.iter manager.Add

let res = manager.ExecuteBlocking<ConsoleContext>(new ConsoleContext("hello")) |> Async.AwaitTask |> Async.RunSynchronously 

if res.Success = false then
    printf "%s" (res.Unfold().Message)