﻿namespace Wing

open System.Threading.Tasks

[<RequireQualifiedAccess>]
module Result =
    //
    // Option

    let bindOption (fn : 'T -> Result<'TOut option, 'TError>) (x : Result<'T option, 'TError>)
        : Result<'TOut option, 'TError> =
        match x with
        | Ok (Some inner) -> fn inner
        | Ok None -> Ok None
        | Error e -> Error e

    let mapOption (fn : 'T -> 'TOut) (x : Result<'T option, 'TError>)
        : Result<'TOut option, 'TError> =
        match x with
        | Ok (Some inner) -> Ok (Some (fn inner))
        | Ok None -> Ok None
        | Error e -> Error e

    let defaultValue (value : 'T) (x : Result<'T option, 'TError>)
        : Result<'T, 'TError> =
        match x with
        | Ok (Some inner) -> Ok inner
        | Ok None -> Ok value
        | Error e -> Error e

    let defaultValueError (value : 'TError) (x : Result<'T option, 'TError>)
        : Result<'T, 'TError> =
        match x with
        | Ok (Some inner) -> Ok inner
        | Ok None -> Error value
        | Error e -> Error e

    //
    // Task
[<RequireQualifiedAccess>]
module TaskResult =
    let ofResult (x : Result<'T, 'TError>)
        : Task<Result<'T, 'TError>> =
        Task.FromResult x

    let bind (fn : 'T -> Task<Result<'TOut, 'TError>>) (x : Task<Result<'T, 'TError>>)
        : Task<Result<'TOut, 'TError>> =
        task {
            match! x with
            | Ok x -> return! fn x
            | Error e -> return Error e
        }

    let bindOption (fn : 'T -> Task<Result<'TOut option, 'TError>>) (x : Task<Result<'T option, 'TError>>)
        : Task<Result<'TOut option, 'TError>> =
            task {
                match! x with
                | Ok (Some inner) -> return! fn inner
                | Ok None -> return Ok None
                | Error e -> return Error e
            }

    let map (fn : 'T -> 'TOut) (x : Task<Result<'T, 'TError>>)
        : Task<Result<'TOut, 'TError>> =
        bind (fn >> Ok >> ofResult) x

    let mapError (fn : 'TError -> 'TErrorOut) (x : Task<Result<'T, 'TError>>)
        : Task<Result<'T, 'TErrorOut>> =
        task {
            match! x with
            | Ok x -> return Ok x
            | Error e -> return Error (fn e)
        }

    let defaultValue (value : 'T) (x : Task<Result<'T option, 'TError>>)
        : Task<Result<'T, 'TError>> =
        map (Option.defaultValue value) x

    let defaultValueError (value : 'TError) (x : Task<Result<'T option, 'TError>>)
        : Task<Result<'T, 'TError>> =
        task {
            match! x with
            | Ok (Some x) -> return Ok x
            | Ok None -> return Error value
            | Error e-> return Error e
        }