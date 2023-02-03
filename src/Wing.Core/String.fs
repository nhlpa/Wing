namespace Wing

[<RequireQualifiedAccess>]
module StringParse =
    open System

    let inline private tryParseWith (tryParseFunc : string -> bool * _) (input : string) =
        let parsed, parsedResult = tryParseFunc input
        match parsed with
        | true-> Some parsedResult
        | false-> None

    let int16 input = tryParseWith Int16.TryParse input
    let int32 input = tryParseWith Int32.TryParse input
    let int64 input = tryParseWith Int64.TryParse input
    let float input = tryParseWith Double.TryParse input
    let decimal input = tryParseWith Decimal.TryParse input
    let dateTime input = tryParseWith DateTime.TryParse input
    let dateTimeOffset input = tryParseWith DateTimeOffset.TryParse input
    let timeSpan input = tryParseWith TimeSpan.TryParse input
    let guid input = tryParseWith Guid.TryParse input