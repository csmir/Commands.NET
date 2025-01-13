module ExecutionDependencies

open Commands

type public LocalCallerContext() = 
    interface ICallerContext with 
        member this.Respond (message: obj): unit = printfn "%s" (message.ToString());