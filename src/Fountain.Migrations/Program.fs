open Fountain.Migrations.Runner

open System.IO
open System
open Argu 


type CliError =
    | ArgumentsNotSpecified

type FountainArgs =
    | [<AltCommandLine("-p")>] Ping
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Ping -> "Can you reach the db???"

let getExitCode result =
    match result with
    | Ok () -> 0
    | Error err ->
        match err with
        | ArgumentsNotSpecified -> 1

let runPrint print = 
    printfn "%s" print
    Ok ()

[<EntryPoint>]
let main argv =
    let connectionString = "Host=localhost; Database=fountain; Username=app; Password=fountain_cli_365;"
    
    //while true do

    //    match canPingDb(connectionString) with
    //    | true -> printfn "Can reach db" 
    //    | false -> printfn "Cannot reach db"

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<FountainArgs>(programName = "fount", errorHandler = errorHandler)
    
    match parser.ParseCommandLine argv with
    | p when p.Contains(Ping) -> 
        match canPingDb(connectionString) with
        | true -> runPrint "Can reach db"
        | false -> runPrint "Cannot reach db"
    | _ ->
        printfn "%s" (parser.PrintUsage())
        Error ArgumentsNotSpecified
    |> getExitCode