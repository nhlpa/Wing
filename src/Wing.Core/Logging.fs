namespace Wing

/// Log input to represent a program error.
type LogError =
    { Error   : exn
      Message : string }

/// The kind of message to output to the log.
type LogMessage =
    | LogError of LogError
    | LogVerbose of string

/// A type to perform logging.
type IAppLogger =
    /// Output the contents of the LogMessage
    abstract member Write : LogMessage -> unit

/// Factory for creating new IAppLogger instances.
type IAppLoggerFactory =
    /// Create a new instance of IAppLogger
    abstract member CreateLogger : unit -> IAppLogger