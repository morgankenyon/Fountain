namespace Fountain.Migrations

module Tool =
    open System

    type MigrationInfo =
        {
            Number : int
            Name : string
            Description : string
        }

    type MigrationRecord =
        {
            MigrationId : int
            Name : string
            Description : string
            DateApplied : DateTime
        }

    type IMigration =
        abstract GetInfo: unit -> MigrationInfo
        abstract Up: unit -> string
        abstract Down: unit -> string

module Runner =
    open Npgsql.FSharp

    type DbInfo = 
        {
            isUp : bool
        }

    let canPingDb (connectionString: string) =
        let checkInfo =
            connectionString
            |> Sql.connect
            |> Sql.query "SELECT FROM pg_database WHERE datname = 'fountain'"
            |> Sql.execute (fun _ ->
                {
                    isUp = true
                })
        not checkInfo.IsEmpty
    
    (*
        SELECT EXISTS (
        SELECT 1
        FROM pg_tables
        WHERE tablename = 'emp_details'
        ) AS table_existence;
    *)
    let checkIfFountainTableExists (connectionString: string) =
        let query = "SELECT EXISTS (
        SELECT 1
        FROM pg_tables
        WHERE tablename = 'dbo.fount'
        ) AS table_exists;"

        connectionString
        |> Sql.connect
        |> Sql.query query
        |> Sql.executeRow (fun read ->
            read.bool "table_exists")

module Db =
    open Tool
    open System

    let getLocalMigrations() =
        let migType = typedefof<IMigration>
        let localMigrations = 
            AppDomain.CurrentDomain.GetAssemblies()
            |> Array.collect (fun a -> a.GetTypes())
            |> Array.filter (fun t -> migType.IsAssignableFrom(t) && not (t.IsInterface))
            |> Array.map (fun mig -> (Activator.CreateInstance mig) :?> IMigration)
            |> Array.sortBy (fun mig -> mig.GetInfo())
        localMigrations

    type InitializeFountain() =
        interface IMigration with
            member this.GetInfo() =
                {
                    Number = 0
                    Name = "InitializeFountain"
                    Description = "Creating tracking table for Fountain"
                }
            member this.Up() = "
                CREATE TABLE dbo.fount(
                    ID              SERIAL PRIMARY KEY,
                    NAME            TEXT NOT NULL,
                    DESCRIPTION     TEXT NOT NULL,
                    DATEAPPLIED     TIMESTAMP WITH TIME ZONE NOT NULL
                );"
            //will need to prompt user (use a special flag) to remove this migration
            //since you lose tracking at that point
            member this.Down() = "DROP TABLE dbo.fount"

    type TestMigration() =
        interface IMigration with
            member this.GetInfo() =
                {
                    Number = 1
                    Name = "TestMigration"
                    Description = "Testing migration"
                }
            member this.Up() = ""
            //will need to prompt user (use a special flag) to remove this migration
            //since you lose tracking at that point
            member this.Down() = ""

    // type InitialMigration() =
    //     interface IMigration with
    //         member this.GetInfo() =
    //             {
    //                 Number = 0
    //                 Name = "Initial"
    //                 Description = "Initial Migration, creating Posts table"
    //             }

    //         member this.Up() = ""
    //         member this.Down() = ""