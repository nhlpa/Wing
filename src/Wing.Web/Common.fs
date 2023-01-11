namespace Wing.Web

open Microsoft.Extensions.Logging
open Validus
open Wing

//
// Logging

/// A type to perform logging via Microsoft.Extensions.Logging.ILogger.
type AppLogger (logger : ILogger) =
    interface IAppLogger with
        member _.Write(logMessage : LogMessage) =
            match logMessage with
            | LogError err -> logger.LogError (err.Error, err.Message)
            | LogVerbose msg -> logger.LogDebug (msg)

/// Factory for creating AppLogger instances.
type AppLoggerFactory (logger : ILogger) =
    interface IAppLoggerFactory with
        member _.CreateLogger() =
            new AppLogger(logger)

//
// Operations

type CommandError =
    | CommandInputError of string list
    | CommandOperationError

type Command<'TInput> = 'TInput -> Result<unit, CommandError>

type QueryError =
    | QueryInputError of string list
    | QueryOperationError
    | NoResult

type Query<'TInput, 'TOutput> = 'TInput -> Result<'TOutput, QueryError>
