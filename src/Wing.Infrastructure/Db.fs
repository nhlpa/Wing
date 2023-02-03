namespace Wing.Infrastructure

open System
open System.Data
open System.Data.Common
open System.Threading.Tasks
open Donald
open Wing

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
    let toLogMessageString (dbError : DbError) =
        let template heading content = String.Concat [|"\n"; heading; ":\n"; string content; "\n" |]
        match dbError with
        | DbConnectionError e -> template "Failed to connect" e.ConnectionString
        | DbTransactionError e -> template "Failed to commit or rollback transaction" e.Step
        | DbExecutionError e -> template "Failed to execute" e.Statement
        | DataReaderCastError e -> template "Failed to read and cast the following field" e.FieldName
        | DataReaderOutOfRangeError e -> template "Failed to read the following field" e.FieldName

    let toLogMessage (dbError : DbError) =
        let logMessage = toLogMessageString dbError
        match dbError with
        | DbConnectionError e -> LogError { Error = e.Error; Message = logMessage }
        | DbTransactionError e -> LogError { Error = e.Error; Message = logMessage }
        | DbExecutionError e -> LogError { Error = e.Error; Message = logMessage }
        | DataReaderCastError e -> LogError { Error = e.Error; Message = logMessage }
        | DataReaderOutOfRangeError e -> LogError { Error = e.Error; Message = logMessage }

module Db =
    let private newDbCommand sql param dbConnection : DbUnit =
        dbConnection |> Db.newCommand sql |> Db.setParams param

    let private logDbCommand (log : LogMessage -> unit) (dbUnit : DbUnit) : DbUnit =
        log (DbUnit.toLogMessage dbUnit)
        dbUnit

    let private logIfError (log : LogMessage -> unit) (dbResult : Result<'a, DbError>) : 'a =
        match dbResult with
        | Ok x -> x
        | Error dbError ->
            log (DbError.toLogMessage dbError)
            match dbError with
            | DbConnectionError e -> raise e.Error
            | DbTransactionError e -> raise e.Error
            | DbExecutionError e -> raise e.Error
            | DataReaderCastError e -> raise e.Error
            | DataReaderOutOfRangeError e -> raise e.Error

    let exec log sql param dbConnection =
        dbConnection
        |> newDbCommand sql param
        |> logDbCommand log
        |> Db.exec
        |> logIfError log

    let read log sql param fn dbConnection =
        dbConnection
        |> newDbCommand sql param
        |> logDbCommand log
        |> Db.read fn
        |> logIfError log

    let query log sql param map dbConnection =
        dbConnection
        |> newDbCommand sql param
        |> logDbCommand log
        |> Db.query map
        |> logIfError log

    let querySingle log sql param map dbConnection =
        dbConnection
        |> newDbCommand sql param
        |> logDbCommand log
        |> Db.querySingle map
        |> logIfError log

    module Async =
        let private throwIfError (log : LogMessage -> unit) (dbResult : Task<Result<_, DbError>>) =
            task {
                let! result = dbResult
                return logIfError log result }

        let exec log sql param dbConnection =
            dbConnection
            |> Db.newCommand sql
            |> Db.setParams param
            |> logDbCommand log
            |> Db.Async.exec
            |> throwIfError log

        let read log sql param fn dbConnection =
            dbConnection
            |> Db.newCommand sql
            |> Db.setParams param
            |> logDbCommand log
            |> Db.Async.read fn
            |> throwIfError log

        let query log sql param map dbConnection =
            dbConnection
            |> Db.newCommand sql
            |> Db.setParams param
            |> logDbCommand log
            |> Db.Async.query map
            |> throwIfError log

        let querySingle log sql param map dbConnection =
            dbConnection
            |> Db.newCommand sql
            |> Db.setParams param
            |> logDbCommand log
            |> Db.Async.querySingle map
            |> throwIfError log


[<AutoOpen>]
module SqlTypeHelpers =
    let inline sqlType (valueFn : 'a -> SqlType) (input : 'a option) =
        match input with
        | Some x -> x |> valueFn
        | None -> SqlType.Null

    let inline sqlBool input = SqlType.Boolean input
    let inline sqlBoolOrNull input = sqlType sqlBool input

    let inline sqlByte input = SqlType.Byte (byte input)
    let inline sqlByteOrNull input = sqlType sqlByte input

    let inline sqlBytes input = SqlType.Bytes input
    let inline sqlBytesOrNull input = sqlType sqlBytes input

    let inline sqlChar input = SqlType.Char (char input)
    let inline sqlCharOrNull input = sqlType sqlChar input

    let inline sqlDateTime input = SqlType.DateTime input
    let inline sqlDateTimeOrNull input = sqlType sqlDateTime input

    let inline sqlDecimal input = SqlType.Decimal (decimal input)
    let inline sqlDecimalOrNull input = sqlType sqlDecimal input

    let inline sqlDouble input = SqlType.Double (double input)
    let inline sqlDoubleOrNull input = sqlType sqlDouble input

    let inline sqlFloat input = SqlType.Float (float input)
    let inline sqlFloatOrNull input = sqlType sqlFloat input

    let inline sqlGuid input = SqlType.Guid input
    let inline sqlGuidOrNull input = sqlType sqlGuid input

    let inline sqlInt16 input = SqlType.Int16 (int16 input)
    let inline sqlInt16OrNull input = sqlType sqlInt16 input

    let inline sqlInt32 input = SqlType.Int32 (int32 input)
    let inline sqlInt32OrNull input = sqlType sqlInt32 input

    let inline sqlInt64 input = SqlType.Int64 (int64 input)
    let inline sqlInt64OrNull input = sqlType sqlInt64 input

    let inline sqlString input = SqlType.String (string input)
    let inline sqlStringOrNull input = sqlType sqlString input