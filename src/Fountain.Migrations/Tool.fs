namespace Fountain.Migrations

module Tool =

    type MigrationInfo =
        {
            Number : int
            Name : string
            Description : string
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

module Db =
    open Tool

    type InitialMigration() =
        interface IMigration with
            member this.GetInfo() =
                {
                    Number = 0
                    Name = "Initial"
                    Description = "Initial Migration, creating Posts table"
                }

            member this.Up() = ""
            member this.Down() = ""