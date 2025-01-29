namespace Commands.Tests

open Commands

type FSharpModule = class 
    inherit CommandModule
        [<Name("echo")>]
        member x.Echo(args: string) = args;

        [<Name("hello")>]
        member x.Hello() = "Hello world"

        [<Name("help")>]
        member x.Help() = 
            for i in x.Manager.GetCommands() do
                printfn "%s" (i.ToString())
end