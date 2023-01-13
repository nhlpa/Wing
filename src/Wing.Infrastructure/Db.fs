namespace Wing.Db

open System
open System.Data
open System.Data.Common
open Donald
open Wing

//
// Interfaces

/// Factory for creating new IDbConnectionFactory instances.
type IDbConnectionFactory =
    abstract member CreateConnection : unit -> IDbConnection

/// Provides ability to perform an action against a database.
type IDbAction =
    abstract member Execute : unit -> Result<unit, DbError>
    abstract member Read : fn : (IDataReader -> 'a) -> Result<'a, DbError>
    abstract member Query : map : (IDataReader -> 'a) -> Result<'a list, DbError>
    abstract member QuerySingle : map : (IDataReader -> 'a) -> Result<'a option, DbError>

/// Factory for creating new IDbAction instances.
type IDbActionFactory =
    abstract member CreateAction : sql : string * ?param : (string * SqlType) list -> IDbAction

/// Provides ability to execute & group actions against a database together, to be
/// saved or undone at run time.
type IDbBatch =
    inherit IDisposable
    inherit IDbActionFactory
    abstract member Save : unit -> unit
    abstract member Undo : unit -> unit

/// Provides ability to execute actions against a database, and create new
/// IDbBatch instances.
type IDbEffect =
    inherit IDisposable
    inherit IDbActionFactory
    abstract member CreateBatch : unit -> IDbBatch

/// A type to represent available interactions with a database.
type IDbContext =
    abstract member CreateUid : unit -> Guid
    abstract member CreateEffect : unit -> IDbEffect

//
// Donald extensions

[<AutoOpen>]
module SqlTypeHelpers =
    let inline sqlType (valueFn : 'a -> SqlType) (input : 'a option) =
        match input with
        | Some x -> x |> valueFn
        | None -> SqlType.Null

    let inline sqlChar input = SqlType.Char (char input)
    let inline sqlCharOrNull input = sqlType sqlChar input

    let inline sqlDecimal input = SqlType.Decimal (decimal input)
    let inline sqlDecimalOrNull input = sqlType sqlDecimal input

    let inline sqlFloat input = SqlType.Float (float input)
    let inline sqlFloatOrNull input = sqlType sqlFloat input

    let inline sqlInt16 input = SqlType.Int16 (int16 input)
    let inline sqlInt16OrNull input = sqlType sqlInt16 input

    let inline sqlInt32 input = SqlType.Int32 (int32 input)
    let inline sqlInt32OrNull input = sqlType sqlInt32 input

    let inline sqlInt64 input = SqlType.Int64 (int64 input)
    let inline sqlInt64OrNull input = sqlType sqlInt64 input

    let inline sqlString input = SqlType.String (string input)
    let inline sqlStringOrNull input = sqlType sqlString input

module internal DbUnit =
    let toDetailString (dbUnit : DbUnit) =
        let cmd = dbUnit.Command
        let param =
            [ for i in 0 .. cmd.Parameters.Count - 1 ->
                let p = cmd.Parameters.[i] :?> DbParameter
                String.Concat [| "@"; p.ParameterName; " = "; string p.Value |] ]
            |> String.concat ", "

        String.Concat [|
            "\n"
            if not(String.IsNullOrWhiteSpace param) then
                "Parameters:\n"
                param
                "\n\n"
            "Command Text:\n"
            cmd.CommandText |]

    let toLogMessage (dbUnit : DbUnit) =
        LogVerbose (toDetailString dbUnit)

module internal DbError =
    let toLogMessage (dbError : DbError) =
        let createLogMessge heading content =
            String.Concat [|"\n"; heading; ":\n"; string content; "\n" |]

        match dbError with
        | DbConnectionError e ->
            LogError {
                Error = e.Error
                Message = createLogMessge "Failed to connect" e.ConnectionString }

        | DbTransactionError e ->
            LogError {
                Error = e.Error
                Message = createLogMessge "Failed to commit or rollback transaction" e.Step }

        | DbExecutionError e ->
            LogError {
                Error = e.Error
                Message = createLogMessge "Failed to execute" e.Statement }

        | DataReaderCastError e ->
            LogError {
                Error = e.Error
                Message = createLogMessge "Failed to read and cast the following field" e.FieldName }

        | DataReaderOutOfRangeError e ->
            LogError {
                Error = e.Error
                Message = createLogMessge "Failed to read the following field" e.FieldName }

//
// Implementations

/// Provides ability to perform an action against a database.
type DbAction (cmd : IDbCommand, logger : IAppLogger) =
    let logCmd (logger : IAppLogger) (dbUnit : DbUnit) =
        logger.Write(DbUnit.toLogMessage dbUnit)
        dbUnit

    let logError (logger : IAppLogger) (result : Result<'a, DbError>) : Result<'a, DbError> =
        result
        |> Result.mapError (fun dbError ->
            logger.Write(DbError.toLogMessage dbError)
            dbError)

    interface IDbAction with
        member _.Execute () =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.exec
            |> logError logger

        member _.Read fn =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.read fn
            |> logError logger

        member _.Query map =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.query map
            |> logError logger

        member _.QuerySingle map =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.querySingle map
            |> logError logger

/// Provides ability to execute & group actions against a database together, to be
/// saved or undone at run time.
type DbBatch (transaction : IDbTransaction, logger : IAppLogger) =
    interface IDbBatch with
        member _.CreateAction (sql, param) =
            let param' = defaultArg param []
            let dbUnit =
                transaction.Connection
                |> Db.newCommand sql
                |> Db.setParams param'
                |> Db.setTransaction transaction

            new DbAction(dbUnit.Command, logger)

        member _.Save () =
            transaction.TryCommit()

        member _.Undo () =
            transaction.TryRollback()

    interface IDisposable with
        member _.Dispose () =
            transaction.Dispose ()

/// Provides ability to execute actions against a database, and create new
/// IDbBatch instances.
type DbEffect (connection : IDbConnection, logger : IAppLogger) =
    interface IDbEffect with
        member _.CreateAction (sql, param) =
            let param' = defaultArg param []
            let dbUnit =
                connection
                |> Db.newCommand sql
                |> Db.setParams param'

            new DbAction(dbUnit.Command, logger)

        member _.CreateBatch () =
            let transaction = connection.TryBeginTransaction ()
            new DbBatch(transaction, logger)

    interface IDisposable with
        member _.Dispose () =
            connection.Dispose ()

/// A type to represent available interactions with a database.
type DbContext (connectionFactory : IDbConnectionFactory, logFactory : IAppLoggerFactory) =
    interface IDbContext with
        member _.CreateUid () = Guid.NewGuid()

        member _.CreateEffect () =
            let logger = logFactory.CreateLogger()
            let connection = connectionFactory.CreateConnection()
            new DbEffect(connection, logger)
