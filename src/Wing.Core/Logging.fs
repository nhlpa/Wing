namespace Wing

/// Log input to represent a program error.
type LogError =
    { Error   : exn
      Message : string }

/// The kind of message to output to the log.
type LogMessage =
    | LogError of LogError
    | LogVerbose of string

/// Process to write a LogMessage
type WriteLogMessage = LogMessage -> unit
