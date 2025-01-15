open Commands
open System

open ExecutionDependencies

//let handler = ResultHandler.Create(Action<LocalCallerContext, IExecuteResult, IServiceProvider>(fun (context: LocalCallerContext) (result: IExecuteResult) (services: IServiceProvider) -> printfn "%A" result))

//let command = Command.Create(Func<string>(fun () -> "Hello world"), [| "hello" |])

//let commands = [| command :> IComponent |]

//let manager = ComponentManager.Create(commands, [| handler |])

//manager.TryExecute<LocalCallerContext>(new LocalCallerContext(), "hello")