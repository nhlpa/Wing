namespace Wing

[<RequireQualifiedAccess>]
module ResultOption =
    let bind (fn : 'T -> Result<'TOut option, 'TError>) (x : Result<'T option, 'TError>) =
        match x with
        | Ok (Some inner) -> fn inner
        | Ok None -> Ok None
        | Error e -> Error e

    let map (fn : 'T -> 'TOut) (x : Result<'T option, 'TError>) =
        match x with
        | Ok (Some inner) -> Ok (Some (fn inner))
        | Ok None -> Ok None
        | Error e -> Error e

    let defaultValue (value : 'T) (x : Result<'T option, 'TError>) =
        match x with
        | Ok (Some inner) -> Ok inner
        | Ok None -> Ok value
        | Error e -> Error e

    let defaultError (value : 'TError) (x : Result<'T option, 'TError>) =
        match x with
        | Ok (Some inner) -> Ok inner
        | Ok None -> Error value
        | Error e -> Error e
