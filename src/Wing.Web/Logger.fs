namespace Wing.Web

open Microsoft.Extensions.Logging
open Wing

/// A type to perform logging via Microsoft.Extensions.Logging.ILogger.
type AppLogger (logger : ILogger) =
    interface IAppLogger with
        member _.Write(logMessage : LogMessage) =
            match logMessage with
            | LogError err -> logger.LogError (err.Error, err.Message)
            | LogVerbose msg -> logger.LogDebug (msg)