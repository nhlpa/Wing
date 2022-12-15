namespace Wing.Db

open System
open System.Data
open System.Data.Common
open Donald
open Wing

type IDbAction =
    abstract member Execute : unit -> Result<unit, CommandError>
    abstract member Query : map : (IDataReader -> 'a) -> Result<'a list, QueryError>
    abstract member QuerySingle : map : (IDataReader -> 'a) -> Result<'a option, QueryError>
    abstract member Read : unit -> Result<IDataReader, QueryError>

type IDbActionFactory =
    abstract member CreateAction : sql : string * ?param : (string * SqlType) list -> IDbAction

type IDbBatch =
    inherit IDisposable
    inherit IDbActionFactory
    abstract member Save : unit -> unit
    abstract member Undo : unit -> unit

type IDbEffect =
    inherit IDisposable
    inherit IDbActionFactory
    abstract member CreateBatch : unit -> IDbBatch

type IDbConnectionFactory =
    abstract member CreateConnection : unit -> IDbConnection

type IDbFixture =
    abstract member CreateUid : unit -> Guid
    abstract member CreateEffect : unit -> IDbEffect

type SecureDbFixture<'TAccount> =
    { Account : 'TAccount
      Fixture : IDbFixture }

module internal DbUnit =
    let toDetailString (dbUnit : DbUnit) =
        let cmd = dbUnit.Command
        let param =
            [ for i in 0 .. cmd.Parameters.Count - 1 ->
                let p = cmd.Parameters.[i] :?> DbParameter
                p.ParameterName, p.Value |> string ]

        sprintf "\nExecuting command:\n%A\n%A\n" param cmd.CommandText

    let toLogMessage (dbUnit : DbUnit) =
        LogVerbose (toDetailString dbUnit)

module internal DbError =
    let toLogMessage (dbError : DbError) =
        let createLogMessge heading content =
            sprintf "\n%s:\n%s\n" heading content

        match dbError with
        | DbConnectionError e ->
            createLogMessge "Failed to connect" e.ConnectionString
            |> fun message -> LogError { Error = e.Error; Message = message }

        | DbTransactionError e ->
            createLogMessge "Failed to commit or rollback transaction" (string e.Step)
            |> fun message -> LogError { Error = e.Error; Message = message }

        | DbExecutionError e ->
            createLogMessge "Failed to execute" e.Statement
            |> fun message -> LogError { Error = e.Error; Message = message }

        | DataReaderCastError e ->
            createLogMessge "Failed to read and cast the following field" e.FieldName
            |> fun message -> LogError { Error = e.Error; Message = message }

        | DataReaderOutOfRangeError e ->
            createLogMessge "Failed to read the following field" e.FieldName
            |> fun message -> LogError { Error = e.Error; Message = message }

    let toOperationError retn (dbError : DbError) =
        match dbError with
        | DbConnectionError _ ->
            retn [ "Could not connect to the database." ]

        | DbTransactionError _ ->
            retn [ "Unable to save changes." ]

        | DbExecutionError _ ->
            retn [ "Unable to execute operation." ]

        | DataReaderCastError _
        | DataReaderOutOfRangeError _ ->
            retn [ "Unable to read data." ]

type DbAction (cmd : IDbCommand, logger : IAppLogger) =
    let logCmd (logger : IAppLogger) (dbUnit : DbUnit) =
        logger.Write(DbUnit.toLogMessage dbUnit)
        dbUnit

    let logError (logger : IAppLogger) (result : Result<'a, DbError>) : Result<'a, DbError> =
        match result with
        | Ok x ->
            Ok x

        | Error dbError ->
            logger.Write(DbError.toLogMessage dbError)
            Error dbError

    interface IDbAction with
        member _.Execute () =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.exec
            |> logError logger
            |> Result.mapError (DbError.toOperationError CommandOperationError)

        member _.Query map =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.query map
            |> logError logger
            |> Result.mapError (DbError.toOperationError QueryOperationError)

        member _.QuerySingle map =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.querySingle map
            |> logError logger
            |> Result.mapError (DbError.toOperationError QueryOperationError)

        member _.Read () =
            new DbUnit(cmd)
            |> logCmd logger
            |> Db.read
            |> logError logger
            |> Result.mapError (DbError.toOperationError QueryOperationError)

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

type DbFixture (connectionFactory : IDbConnectionFactory, logFactory : IAppLoggerFactory) =
    interface IDbFixture with
        member _.CreateUid () = Guid.NewGuid()

        member _.CreateEffect () =
            let logger = logFactory.CreateLogger()
            let connection = connectionFactory.CreateConnection()
            new DbEffect(connection, logger)

module DbBatchResult =
    let saveOrUndo (batch : IDbBatch) (result : Result<'a, 'b>) =
        match result with
        | Ok x ->
            batch.Save ()
            Ok x

        | Error x ->
            batch.Undo ()
            Error x

[<AutoOpen>]
module SqlTypeHelpers =
    let apply (valueFn : 'a -> SqlType) (input : 'a option) =
        match input with
        | Some x -> x |> valueFn
        | None -> SqlType.Null

    let inline sqlBoolean input = SqlType.Boolean (bool input)
    let inline sqlBooleanOrNull input = apply sqlBoolean input

    let inline sqlDateTime input = SqlType.DateTime (datetime input)
    let inline sqlDateTimeOrNull input = apply sqlDateTime input

    let inline sqlInt16 input = SqlType.Int16 (int16 input)
    let inline sqlInt16OrNull input = apply sqlInt16 input

    let inline sqlInt32 input = SqlType.Int32 (int32 input)
    let inline sqlInt32OrNull input = apply sqlInt32 input

    let inline sqlInt64 input = SqlType.Int64 (int64 input)
    let inline sqlInt64OrNull input = apply sqlInt64 input

    let inline sqlDecimal input = SqlType.Decimal (decimal input)
    let inline sqlDecimalOrNull input = apply sqlDecimal input

    let inline sqlChar input = SqlType.Char input
    let inline sqlCharOrNull input = apply sqlChar input

    let inline sqlString input = SqlType.String (string input)
    let inline sqlStringOrNull input = apply sqlString input

    let inline sqlGuid (input : ^a when ^a : (member ToGuid : unit -> Guid)) = SqlType.Guid (guid input)
    let inline sqlGuidOrNull (input : ^a option when ^a : (member ToGuid : unit -> Guid)) = apply sqlGuid input
