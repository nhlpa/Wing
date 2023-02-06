namespace Wing

[<AutoOpen>]
module Extensions =
    open System
    open System.Text.RegularExpressions

    type String with
        member x.IsEmpty () = String.IsNullOrWhiteSpace(x)
        member x.IsNotEmpty () = not (x.IsEmpty())
        member x.ReplacePattern (pattern : string, replace : string) = Regex.Replace(input = x, pattern = pattern, replacement = replace)

    let inline private tryParseWith (tryParseFunc : string -> bool * _) (input : string) =
        let parsed, parsedResult = tryParseFunc input
        if parsed then Some parsedResult else None

    type Int16 with
        static member TryParseOption (input : string) = tryParseWith Int16.TryParse input

    type Int32 with
        static member TryParseOption (input : string) = tryParseWith Int32.TryParse input

    type Int64 with
        static member TryParseOption (input : string) = tryParseWith Int64.TryParse input

    type Double with
        static member TryParseOption (input : string) = tryParseWith Double.TryParse input

    type Decimal with
        static member TryParseOption (input : string) = tryParseWith Decimal.TryParse input

    type DateTime with
        static member TryParseOption (input : string) = tryParseWith DateTime.TryParse input

    type DateTimeOffset with
        static member TryParseOption (input : string) = tryParseWith DateTimeOffset.TryParse input

    type TimeSpan with
        static member TryParseOption (input : string) = tryParseWith TimeSpan.TryParse input

    type Guid with
        static member TryParseOption (input : string) = tryParseWith Guid.TryParse input
