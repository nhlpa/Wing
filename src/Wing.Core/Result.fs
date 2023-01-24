namespace Wing

[<RequireQualifiedAccess>]
module Result =
    let bindOption (fn : 'T -> Result<'TOut option, 'TError>) (x : Result<'T option, 'TError>) =
        match x with
        | Ok (Some inner) -> fn inner
        | Ok None -> Ok None
        | Error e -> Error e

    let mapOption (fn : 'T -> 'TOut) (x : Result<'T option, 'TError>) =
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

[<RequireQualifiedAccess>]
module TaskResult =
    open System.Threading.Tasks

    let map (fn : 'T -> 'TOut) (x : Task<Result<'T, 'TError>>) : Task<Result<'TOut, 'TError>> =
        task {
            match! x with
            | Ok x -> return Ok (fn x)
            | Error e -> return Error e
        }

    let mapError (fn : 'TError -> 'TErrorOut) (x : Task<Result<'T, 'TError>>) : Task<Result<'T, 'TErrorOut>> =
        task {
            match! x with
            | Ok x -> return Ok x
            | Error e -> return Error (fn e)
        }

    let bindResult (fn : 'T -> Task<Result<'TOut, 'TError>>) (x : Result<'T, 'TError>) =
        match x with
        | Ok x ->
            task {
                return! fn x
            }
        | Error e -> Task.FromResult (Error e)
