open Fountain.Migrations.Runner
open Fountain.Migrations.Db

open System.IO
open System
open Argu 


type CliError =
    | ArgumentsNotSpecified

type FountainArgs =
    | [<AltCommandLine("-p")>] Ping
    | [<AltCommandLine("-v")>] Verify
    | [<AltCommandLine("-ll")>] LocalMigrations
    //| [<AltCommandLine("-dbm")>] DbMigrations
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Ping -> "Can you reach the db???"
            | Verify -> "Verify that you're connecting to a Fountain Database"
            | LocalMigrations -> "List local migrations"
            //| DbMigration -> "List db migrations"

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

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<FountainArgs>(programName = "fount", errorHandler = errorHandler)
    
    match parser.ParseCommandLine argv with
    | p when p.Contains(Ping) -> 
        match canPingDb(connectionString) with
        | true -> runPrint "Can reach db"
        | false -> runPrint "Cannot reach db"
    | p when p.Contains(Verify) ->
        match checkIfFountainTableExists(connectionString) with
        | true -> runPrint "Connecting to a Fountain db"
        | false -> runPrint "Database is not a Fountain db"
    | p when p.Contains(LocalMigrations) ->
        let migrations = getLocalMigrations()
        for mig in migrations do
            let name = mig.GetInfo().Name
            printfn "%s" name
        Ok ()
    | _ ->
        printfn "%s" (parser.PrintUsage())
        Error ArgumentsNotSpecified
    |> getExitCode