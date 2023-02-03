namespace Wing

[<AutoOpen>]
module OptionBuilder =
    [<Struct>]
    type OptionBuilder =
        member _.Return (x) =  Ok x
        member _.ReturnFrom (x) = x
        member _.Bind (x, fn) = Option.bind fn x
        member _.Zero () = Ok ()
        member _.Combine (x, fn) = Option.bind fn x
        member _.Delay (x) = x
        member _.Run (fn) = fn ()

    let option = OptionBuilder()
