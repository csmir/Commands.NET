open Commands
open System

open ExecutionDependencies

let handler = new DelegateResultHandler<ICallerContext>(ResultProcessor.Process())

let command = Command.Create(Func<string>(fun () -> "Hello world"), [| "hello" |])

let handlers = [| handler :> ResultHandler |]
let commands = [| command :> IComponent |]

let manager = ComponentManager.Create(commands, handlers)

manager.TryExecute<LocalCallerContext>(new LocalCallerContext(), "hello")