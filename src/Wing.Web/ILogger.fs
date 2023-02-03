namespace Wing.Web

open Microsoft.Extensions.Logging
open Wing

[<AutoOpen>]
module Logging =
    /// A type to perform logging via Microsoft.Extensions.Logging.ILogger.
    type ILogger with
        member x.WriteLogMessage(logMessage : LogMessage) =
            match logMessage with
            | LogError err -> x.LogError(err.Error, err.Message)
            | LogVerbose msg -> x.LogDebug(msg)